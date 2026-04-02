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
            var eventHubQueueBuilder = new EventHubBuilder();

            builder?.Invoke(eventHubQueueBuilder);

            services.AddSingleton<IValidateOptions<EventHubOptions>, EventHubOptionsValidator>();

            foreach (var pair in eventHubQueueBuilder.EventHubQueueConfigureOptions)
            {
                services.AddOptions<EventHubOptions>(pair.Key).Configure(options =>
                {
                    pair.Value(options);
                });
            }

            services.AddSingleton<ITransportFactory, EventHubFactory>();

            return hopperBuilder;
        }
    }
}