using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Primitives;
using Azure.Messaging.EventHubs.Processor;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Shuttle.Core.Contract;
using Shuttle.Core.Streams;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHub : ITransport, IPurgeTransport, IDisposable
{
    private readonly HopperOptions _hopperOptions;
    private readonly TransportOperationEventArgs _acknowledgeStartingEventArgs;
    private readonly TransportOperationEventArgs _processEventHandlerOperationMessageReceivedEventArgs;
    private readonly TransportOperationEventArgs _processEventHandlerOperationNoMessageReceivedEventArgs;

    private readonly BlobContainerClient? _blobContainerClient;
    private readonly EventHubOptions _eventHubOptions;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly EventProcessorClient? _processorClient;
    private readonly EventHubProducerClient _producerClient;
    private readonly string _clientIdentifier;

    private readonly Queue<ReceivedMessage> _receivedMessages = new();
    private ProcessEventArgs? _acknowledgeProcessEventArgs;

    private int _checkpointItem = 1;
    private bool _disposed;
    private bool _started;

    public EventHub(HopperOptions hopperOptions, EventHubOptions eventHubOptions, TransportUri uri)
    {
        _hopperOptions = Guard.AgainstNull(hopperOptions);
        _eventHubOptions = Guard.AgainstNull(eventHubOptions);

        Uri = Guard.AgainstNull(uri);

        _acknowledgeStartingEventArgs = new(this, "[acknowledge/starting]");
        _processEventHandlerOperationMessageReceivedEventArgs = new(this, "[process-event-handler/message-received]");
        _processEventHandlerOperationNoMessageReceivedEventArgs = new(this, "[process-event-handler/no-message-received]");

        _clientIdentifier = string.IsNullOrWhiteSpace(_eventHubOptions.ClientIdentifier)
            ? $"{Assembly.GetEntryAssembly()?.GetName().Name ?? "EventHubClient"}-{Environment.MachineName}-{Process.GetCurrentProcess().Id}"
            : _eventHubOptions.ClientIdentifier;

        _producerClient = new(_eventHubOptions.ConnectionString, Uri.TransportName, eventHubOptions.ProducerClient ?? new());

        if (!_eventHubOptions.ProcessEvents)
        {
            return;
        }

        _blobContainerClient = new(_eventHubOptions.BlobStorageConnectionString, _eventHubOptions.BlobContainerName, _eventHubOptions.BlobClient ?? new());
        _processorClient = new(_blobContainerClient, _eventHubOptions.ConsumerGroup, _eventHubOptions.ConnectionString, uri.TransportName, _eventHubOptions.ProcessorClient ?? new());

        _processorClient.ProcessEventAsync += ProcessEventHandler;
        _processorClient.ProcessErrorAsync += ProcessErrorHandlerAsync;
        _processorClient.PartitionInitializingAsync += InitializeEventHandler;
    }

    public TransportUri Uri { get; }

    public void Dispose()
    {
        _lock.Wait(CancellationToken.None);

        try
        {
            if (_disposed)
            {
                return;
            }

            _producerClient.DisposeAsync().AsTask().Wait(_eventHubOptions.OperationTimeout);

            _acknowledgeProcessEventArgs?.UpdateCheckpointAsync(CancellationToken.None).GetAwaiter().GetResult();

            if (_processorClient != null)
            {
                try
                {
                    _hopperOptions.TransportOperation.InvokeAsync(new(this, "[dispose/stop-processing/starting]")).GetAwaiter().GetResult();

                    _processorClient.StopProcessing(CancellationToken.None);
                }
                finally
                {
                    _hopperOptions.TransportOperation.InvokeAsync(new(this, "[dispose/stop-processing/completed]")).GetAwaiter().GetResult();
                }

                _processorClient.PartitionInitializingAsync -= InitializeEventHandler;
                _processorClient.ProcessEventAsync -= ProcessEventHandler;
                _processorClient.ProcessErrorAsync -= ProcessErrorHandlerAsync;
            }

            _disposed = true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AcknowledgeAsync(object acknowledgementToken, CancellationToken cancellationToken = default)
    {
        if (Guard.AgainstNull(acknowledgementToken) is not ProcessEventArgs args)
        {
            return;
        }

        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        try
        {
            if (_checkpointItem == _eventHubOptions.CheckpointInterval)
            {
                await _hopperOptions.TransportOperation.InvokeAsync(_acknowledgeStartingEventArgs, cancellationToken);

                await args.UpdateCheckpointAsync(cancellationToken).ConfigureAwait(false);

                _acknowledgeProcessEventArgs = null;
                _checkpointItem = 1;
            }
            else
            {
                _acknowledgeProcessEventArgs = args;
                _checkpointItem++;
            }
        }
        finally
        {
            _lock.Release();
        }

        await _hopperOptions.TransportOperation.InvokeAsync(new(this, "[acknowledge/cancelled]"), cancellationToken);
    }

    private async Task BufferAsync(CancellationToken cancellationToken)
    {
        if (_processorClient == null)
        {
            return;
        }

        if (!_started)
        {
            await _processorClient.StartProcessingAsync(cancellationToken);

            _started = true;
        }

        if (_eventHubOptions.ConsumeTimeout <= TimeSpan.Zero)
        {
            return;
        }

        var timeout = DateTime.Now.Add(_eventHubOptions.ConsumeTimeout);

        while (_receivedMessages.Count == 0 && timeout > DateTime.Now && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(250, cancellationToken);
        }
    }

    public async Task SendAsync(TransportMessage transportMessage, Stream stream, CancellationToken cancellationToken = default)
    {
        Guard.AgainstNull(transportMessage);
        Guard.AgainstNull(stream);

        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        if (_disposed)
        {
            return;
        }

        try
        {
            await _producerClient.SendAsync([new(Convert.ToBase64String(await stream.ToBytesAsync().ConfigureAwait(false)))], cancellationToken);
        }
        finally
        {
            _lock.Release();
        }

        await _hopperOptions.MessageSent.InvokeAsync(new(this, transportMessage, stream), cancellationToken);
    }

    public TransportType Type => TransportType.Stream;

    public async Task<ReceivedMessage?> ReceiveAsync(CancellationToken cancellationToken = default)
    {
        if (!_eventHubOptions.ProcessEvents)
        {
            return null;
        }

        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        ReceivedMessage? receivedMessage;

        try
        {
            await BufferAsync(cancellationToken);

            receivedMessage = _receivedMessages.Count > 0 && !_disposed ? _receivedMessages.Dequeue() : null;
        }
        finally
        {
            _lock.Release();
        }

        if (receivedMessage != null)
        {
            await _hopperOptions.MessageReceived.InvokeAsync(new(this, receivedMessage), cancellationToken);
        }

        return receivedMessage;
    }

    private async Task InitializeEventHandler(PartitionInitializingEventArgs args)
    {
        if (args.CancellationToken.IsCancellationRequested)
        {
            return;
        }

        args.DefaultStartingPosition = _eventHubOptions.DefaultStartingPosition;

        await Task.CompletedTask;
    }

    public async ValueTask<bool> HasPendingAsync(CancellationToken cancellationToken = default)
    {
        if (!_eventHubOptions.ProcessEvents)
        {
            await _hopperOptions.TransportOperation.InvokeAsync(new(this, "[has-pending]", false), cancellationToken);

            return true;
        }

        await _hopperOptions.TransportOperation.InvokeAsync(new(this, "[has-pending/starting]"), cancellationToken);

        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        bool result;

        try
        {
            await BufferAsync(cancellationToken);

            result = _receivedMessages.Count > 0;
        }
        finally
        {
            _lock.Release();
        }

        await _hopperOptions.TransportOperation.InvokeAsync(new(this, "[has-pending]", result), cancellationToken);

        return result;
    }

    private async Task ProcessErrorHandlerAsync(ProcessErrorEventArgs args)
    {
        await  _eventHubOptions.ProcessError.InvokeAsync(new(this, args));
    }

    private async Task ProcessEventHandler(ProcessEventArgs args)
    {
        if (args.HasEvent)
        {
            _receivedMessages.Enqueue(new(new MemoryStream(Convert.FromBase64String(Encoding.UTF8.GetString(args.Data.Body.ToArray()))), args));
            await _hopperOptions.TransportOperation.InvokeAsync(_processEventHandlerOperationMessageReceivedEventArgs);
        }
        else
        {
            await _hopperOptions.TransportOperation.InvokeAsync(_processEventHandlerOperationNoMessageReceivedEventArgs);
        }

        await Task.CompletedTask;
    }

    public async Task PurgeAsync(CancellationToken cancellationToken = default)
    {
        await _hopperOptions.TransportOperation.InvokeAsync(new(this, "[purge/starting]"), cancellationToken);

        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        try
        {
            if (!_eventHubOptions.ProcessEvents)
            {
                return;
            }

            if (_eventHubOptions.DefaultStartingPosition != EventPosition.Latest)
            {
                throw new ApplicationException(string.Format(Resources.UnsupportedPurgeException, Uri.Uri));
            }

            var checkpointStore = new BlobCheckpointStore(_blobContainerClient);

            foreach (var partitionId in await _producerClient.GetPartitionIdsAsync(cancellationToken))
            {
                var partitionProperties = await _producerClient.GetPartitionPropertiesAsync(partitionId, cancellationToken);

                await checkpointStore.UpdateCheckpointAsync(_producerClient.FullyQualifiedNamespace, Uri.TransportName, _eventHubOptions.ConsumerGroup, partitionId, _clientIdentifier, new(partitionProperties.LastEnqueuedOffsetString, partitionProperties.LastEnqueuedSequenceNumber), cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _lock.Release();
        }

        await _hopperOptions.TransportOperation.InvokeAsync(new(this, "[purge/completed]"), cancellationToken);
    }

    public async Task ReleaseAsync(object acknowledgementToken, CancellationToken cancellationToken = default)
    {
        if (Guard.AgainstNull(acknowledgementToken) is not ProcessEventArgs args)
        {
            return;
        }

        await _lock.WaitAsync(CancellationToken.None).ConfigureAwait(false);

        try
        {
            _receivedMessages.Enqueue(new(new MemoryStream(Convert.FromBase64String(Encoding.UTF8.GetString(args.Data.Body.ToArray()))), args));
        }
        finally
        {
            _lock.Release();
        }

        await _hopperOptions.MessageReleased.InvokeAsync(new(this, acknowledgementToken), cancellationToken);
    }
}