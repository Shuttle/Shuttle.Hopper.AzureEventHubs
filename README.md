# Shuttle.Hopper.AzureEventHubs

Azure Event Hubs implementation for use with Shuttle.Hopper.

## Installation

```bash
dotnet add package Shuttle.Hopper.AzureEventHubs
```

## Configuration

The URI structure is `azureeh://configuration-name/hub-name`.

```c#
services.AddHopper()
    .UseAzureEventHubs(builder =>
    {
        builder.Configure("azure", options => 
        {
            options.ConnectionString = "Endpoint=sb://{hub-namespace}.servicebus.windows.net/;SharedAccessKeyName={key-name};SharedAccessKey={key};EntityPath={hub-name}";
            options.BlobStorageConnectionString = "{BlobStorageConnectionString}";
            options.ProcessorClient = new() { PrefetchCount = 100 };
            options.ProcessEvents = true;
            options.ConsumerGroup = "$Default";
            options.BlobContainerName = "{BlobContainerName}";
            options.OperationTimeout = TimeSpan.FromSeconds(30);
            options.ConsumeTimeout = TimeSpan.FromSeconds(30);
            options.DefaultStartingPosition = EventPosition.Latest;
            options.CheckpointInterval = 1;

            options.ProcessError.Register(async args =>
            {
                Console.WriteLine($"[error] : {args.Exception.Message}");
                await Task.CompletedTask;
            });
        });
    });
```

The default JSON settings structure is as follows:

```json
{
  "Shuttle": {
    "AzureEventHubs": {
      "azure": {
        "ConnectionString": "Endpoint=sb://{hub-namespace}.servicebus.windows.net/;SharedAccessKeyName={key-name};SharedAccessKey={key};EntityPath={hub-name}",
        "ProcessEvents": false,
        "ConsumerGroup": "$Default",
        "BlobStorageConnectionString": "{BlobStorageConnectionString}",
        "BlobContainerName": "{BlobContainerName}",
        "OperationTimeout": "00:00:30",
        "ConsumeTimeout": "00:00:30",
        "DefaultStartingPosition": "Latest",
        "CheckpointInterval": 1
      }
    }
  }
}
```

## `EventHubOptions`

| Segment / Argument | Default | Description |
| --- | --- | --- | 
| `ConnectionString` | | The Azure Event Hubs endpoint to connect to. |
| `ProcessEvents` | `false` | Indicates whether the endpoint will process messages.  If `true`, an `EventProcessorClient` is instanced and configured. |
| `ConsumerGroup` | "$Default" | The consumer group to use when processing events. |
| `BlobStorageConnectionString` | | The Azure Storage Account endpoint to connect to in order to perform checkpoints. |
| `BlobContainerName` | | The blob container name where checkpoints will be stored. |
| `OperationTimeout` | "00:00:30" | The duration to wait for relevant `async` methods to complete before timing out. |
| `ConsumeTimeout` | "00:00:30" | The duration to poll for messages before returning `null`. |
| `DefaultStartingPosition` | `Latest` | The default starting position to use when no checkpoint exists. |
| `CheckpointInterval` | `1` | The number of events to process before performing a checkpoint. |
| `ClientIdentifier` | | A unique identifier for the client. |
| `BlobClient` | | The `BlobClientOptions` used to configure the underlying `BlobContainerClient`. |
| `ProcessorClient` | | The `EventProcessorClientOptions` used to configure the underlying `EventProcessorClient`. |
| `ProducerClient` | | The `EventHubProducerClientOptions` used to configure the underlying `EventHubProducerClient`. |
| `ProcessError` | | The `AsyncEvent<EventHubProcessErrorEventArgs>` triggered when an error occurs during event processing. |
