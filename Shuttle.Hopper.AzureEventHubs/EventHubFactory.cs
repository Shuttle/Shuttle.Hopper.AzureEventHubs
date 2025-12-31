using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubFactory(IOptions<HopperOptions> hopperOptions, IOptionsMonitor<EventHubOptions> eventHubQueueOptions) : ITransportFactory
{
    private readonly HopperOptions _hopperOptions = Guard.AgainstNull(Guard.AgainstNull(hopperOptions).Value);
    private readonly IOptionsMonitor<EventHubOptions> _eventHubQueueOptions = Guard.AgainstNull(eventHubQueueOptions);

    public string Scheme => "azureeh";

    public Task<ITransport> CreateAsync(Uri uri, CancellationToken cancellationToken = default)
    {
        var transportUri = new TransportUri(Guard.AgainstNull(uri)).SchemeInvariant(Scheme);
        var eventHubQueueOptions = _eventHubQueueOptions.Get(transportUri.ConfigurationName);

        if (eventHubQueueOptions == null)
        {
            throw new InvalidOperationException(string.Format(Hopper.Resources.TransportConfigurationNameException, transportUri.ConfigurationName));
        }

        return Task.FromResult<ITransport>(new EventHub(_hopperOptions, eventHubQueueOptions, transportUri));
    }
}