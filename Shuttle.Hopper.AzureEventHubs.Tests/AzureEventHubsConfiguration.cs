using Azure.Messaging.EventHubs.Consumer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.Hopper.AzureEventHubs.Tests;

public class AzureEventHubsConfiguration
{
    public static IServiceCollection GetServiceCollection()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder().AddUserSecrets<AzureEventHubsConfiguration>().Build();

        services.AddSingleton<IConfiguration>(configuration);

        services.AddHopper()
            .UseAzureEventHubs(builder =>
            {
                builder.Configure("azure", options =>
                {
                    options.ConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";
                    options.BlobStorageConnectionString = "UseDevelopmentStorage=true";
                    options.ProcessorClient = new() { PrefetchCount = 100 };
                    options.ProcessEvents = true;
                    options.ConsumerGroup = "$Default";
                    options.BlobContainerName = "eh-shuttle-hopper";
                    options.OperationTimeout = TimeSpan.FromSeconds(5);
                    options.ConsumeTimeout = TimeSpan.FromSeconds(15);
                    options.DefaultStartingPosition = EventPosition.Latest;
                    options.CheckpointInterval = 5;
                });
            });

        return services;
    }
}