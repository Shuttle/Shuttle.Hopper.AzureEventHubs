using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Messaging.EventHubs.Producer;
using Azure.Storage.Blobs;
using Shuttle.Extensions.Options;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubOptions
{
    public const string SectionName = "Shuttle:AzureEventHubs";

    public BlobClientOptions? BlobClient { get; set; }
    public string BlobContainerName { get; set; } = string.Empty;

    public string BlobStorageConnectionString { get; set; } = string.Empty;
    public int CheckpointInterval { get; set; } = 1;
    public string ClientIdentifier { get; set; } = string.Empty;

    public string ConnectionString { get; set; } = string.Empty;
    public string ConsumerGroup { get; set; } = EventHubConsumerClient.DefaultConsumerGroupName;
    public TimeSpan ConsumeTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public EventPosition DefaultStartingPosition { get; set; } = EventPosition.Latest;
    public TimeSpan OperationTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public AsyncEvent<EventHubProcessErrorEventArgs> ProcessError { get; set; } = new();
    public bool ProcessEvents { get; set; }
    public EventProcessorClientOptions? ProcessorClient { get; set; }
    public EventHubProducerClientOptions? ProducerClient { get; set; }
}