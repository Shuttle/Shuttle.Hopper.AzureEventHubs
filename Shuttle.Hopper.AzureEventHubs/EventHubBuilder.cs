using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubBuilder
{
    internal readonly Dictionary<string, Action<EventHubOptions>> EventHubQueueConfigureOptions = new();

    public EventHubBuilder Configure(string name, Action<EventHubOptions> configureOptions)
    {
        Guard.AgainstEmpty(name);
        Guard.AgainstNull(configureOptions);

        EventHubQueueConfigureOptions.Remove(name);
        EventHubQueueConfigureOptions.Add(name, configureOptions);

        return this;
    }
}