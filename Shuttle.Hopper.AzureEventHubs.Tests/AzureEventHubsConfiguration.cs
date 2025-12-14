using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Hopper.AzureEventHubs.Tests;

public class AzureEventHubsConfiguration
{
    public static IServiceCollection GetServiceCollection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder().AddUserSecrets<AzureEventHubsConfiguration>().Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddAzureEventHubs(builder =>
        {
            var eventHubQueueOptions = new EventHubOptions
            {
                ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;",
                BlobStorageConnectionString = "UseDevelopmentStorage=true",
                ProcessorClient = new()
                {
                    PrefetchCount = 100
                },
                ProcessEvents = true,
                ConsumerGroup = "$Default",
                BlobContainerName = "eh-shuttle-hopper",
                OperationTimeout = TimeSpan.FromSeconds(5),
                ConsumeTimeout = TimeSpan.FromSeconds(15),
                DefaultStartingPosition = EventPosition.Latest,
                CheckpointInterval = 5
            };

            configuration.GetSection($"{EventHubOptions.SectionName}:azure").Bind(eventHubQueueOptions);

            builder.AddOptions("azure", eventHubQueueOptions);
        });

        return services;
    }
}