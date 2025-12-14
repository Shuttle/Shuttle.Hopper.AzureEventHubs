using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubBuilder(IServiceCollection services)
{
    internal readonly Dictionary<string, EventHubOptions> EventHubQueueOptions = new();

    public IServiceCollection Services { get; } = Guard.AgainstNull(services);

    public EventHubBuilder AddOptions(string name, EventHubOptions eventHubOptions)
    {
        Guard.AgainstEmpty(name);
        Guard.AgainstNull(eventHubOptions);

        EventHubQueueOptions.Remove(name);

        EventHubQueueOptions.Add(name, eventHubOptions);

        return this;
    }
}