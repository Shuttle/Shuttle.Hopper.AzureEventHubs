using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Hopper.AzureEventHubs;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAzureEventHubs(Action<EventHubBuilder>? builder = null)
        {
            var eventHubQueueBuilder = new EventHubBuilder(Guard.AgainstNull(services));

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

            services.TryAddSingleton<ITransportFactory, EventHubFactory>();

            return services;
        }
    }
}