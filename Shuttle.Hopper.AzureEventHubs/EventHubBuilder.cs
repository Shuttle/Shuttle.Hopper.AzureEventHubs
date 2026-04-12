using Microsoft.Extensions.DependencyInjection;
using Shuttle.Contract;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubBuilder(IServiceCollection services)
{
    public EventHubBuilder Configure(string name, Action<EventHubOptions> configureOptions)
    {
        Guard.AgainstNull(services)
            .AddOptions<EventHubOptions>(Guard.AgainstEmpty(name))
            .Configure(Guard.AgainstNull(configureOptions));
        
        return this;
    }
}