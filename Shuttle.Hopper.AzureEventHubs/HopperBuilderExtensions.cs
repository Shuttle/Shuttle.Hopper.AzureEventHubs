using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Shuttle.Hopper.AzureEventHubs;

public static class HopperBuilderExtensions
{
    extension(HopperBuilder hopperBuilder)
    {
        public IServiceCollection UseAzureEventHubs(Action<EventHubBuilder>? builder = null)
        {
            var services = hopperBuilder.Services;
            var eventHubQueueBuilder = new EventHubBuilder(services);

            builder?.Invoke(eventHubQueueBuilder);

            services.AddSingleton<IValidateOptions<EventHubOptions>, EventHubOptionsValidator>();

            foreach (var pair in eventHubQueueBuilder.EventHubQueueOptions)
            {
                services.AddOptions<EventHubOptions>(pair.Key).Configure(options =>
                {
                    options.BlobClient = pair.Value.BlobClient;
                    options.ProducerClient = pair.Value.ProducerClient;
                    options.ProcessorClient = pair.Value.ProcessorClient;

                    options.ConnectionString = pair.Value.ConnectionString;
                    options.ProcessEvents = pair.Value.ProcessEvents;
                    options.BlobStorageConnectionString = pair.Value.BlobStorageConnectionString;
                    options.BlobContainerName = pair.Value.BlobContainerName;
                    options.ConsumerGroup = pair.Value.ConsumerGroup;
                    options.OperationTimeout = pair.Value.OperationTimeout;
                    options.ConsumeTimeout = pair.Value.ConsumeTimeout;
                    options.DefaultStartingPosition = pair.Value.DefaultStartingPosition;
                    options.CheckpointInterval = pair.Value.CheckpointInterval;
                });
            }

            services.AddSingleton<ITransportFactory, EventHubFactory>();

            return services;
        }
    }
}