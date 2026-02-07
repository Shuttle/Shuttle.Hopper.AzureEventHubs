# Shuttle.Hopper.AzureEventHubs

Azure Event Hubs implementation for use with Shuttle.Hopper.

## Installation

```bash
dotnet add package Shuttle.Hopper.AzureEventHubs
```

## Configuration

The URI structure is `azureeh://configuration-name/hub-name`.

```c#
services.AddHopper(builder =>
{
    builder.UseAzureEventHubs(eventHubBuilder =>
    {
        var eventHubOptions = new EventHubOptions
        {
            ConnectionString = "Endpoint=sb://{hub-namespace}.servicebus.windows.net/;SharedAccessKeyName={key-name};SharedAccessKey={key};EntityPath={hub-name}",
            ProcessEvents = true,
            ConsumerGroup = "$Default",
            BlobStorageConnectionString = "{BlobStorageConnectionString}",
            BlobContainerName = "{BlobContainerName}",
            OperationTimeout = TimeSpan.FromSeconds(30),
            ConsumeTimeout = TimeSpan.FromSeconds(30),
            DefaultStartingPosition = EventPosition.Latest,
            CheckpointInterval = 1
        };

        eventHubOptions.ProcessError.Register(async args =>
        {
            Console.WriteLine($"[error] : {args.Exception.Message}");
            await Task.CompletedTask;
        });

        eventHubBuilder.AddOptions("azure", eventHubOptions);
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

## Options

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
