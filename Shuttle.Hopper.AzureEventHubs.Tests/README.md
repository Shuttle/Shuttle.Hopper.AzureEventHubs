# Fixture configuration

## Event Hubs Emulator

You can make use of hte [Event Hubs emulator](https://learn.microsoft.com/en-us/azure/event-hubs/test-locally-with-event-hub-emulator?tabs=docker-linux-container%2Cusing-kafka) to develop locally.

See the `eventhubs-emulator.cmd` file for the Docker command to create the instance, which requires a network called `development` as well as [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite) running within the same network.

The `Config.json` file conain the definition for these event hubs:

- `test-error`
- `test-inbox-work`

However, plase use [Azure Storage explorer](https://azure.microsoft.com/en-us/products/storage/storage-explorer) to create the `eh-shuttle-hopper` blob container which is used for checkpoints.

## Azure

You will need to create an Event Hubs [namespace](https://portal.azure.com/#view/HubsExtension/BrowseResource/resourceType/Microsoft.EventHub%2Fnamespaces) with a [shared access policy](https://portal.azure.com/#@bscglobal.com/resource/subscriptions/020717a5-b2a3-4630-915f-ca09a8df7c75/resourceGroups/rg-shuttle/providers/Microsoft.EventHub/namespaces/shuttle/saskey)] with `Send` and `Listen` claims.

The fixtures will need the following event hubs to be preent:

- `test-error`
- `test-inbox-work`

In addition, you need to create a `Container` with name `eh-shuttle-hopper` in a `Storage Account` to store the checkpoints.

Right-click on the `Shuttle.Hopper.AzureEventHubs.Tests` project and select `Manage User Secrets` and add the following configuration:

```json
{
    "Shuttle": {
        "AzureEventHubs": {
          "azure": {
            "ConnectionString": "displayed when you click on the shared access policy",
            "BlobStorageConnectionString": ""
          }
        }
    }
}
```