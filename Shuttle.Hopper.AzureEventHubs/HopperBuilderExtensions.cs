using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shuttle.Hopper.AzureEventHubs;

public static class HopperBuilderExtensions
{
    extension(HopperBuilder hopperBuilder)
    {
        public HopperBuilder UseAzureEventHubs(Action<EventHubBuilder>? builder = null)
        {
            var services = hopperBuilder.Services;

            builder?.Invoke(new(services));

            services.AddSingleton<IValidateOptions<EventHubOptions>, EventHubOptionsValidator>();
            services.AddSingleton<ITransportFactory, EventHubFactory>();

            return hopperBuilder;
        }
    }
}