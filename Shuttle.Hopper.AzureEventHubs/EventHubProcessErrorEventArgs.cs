using Azure.Messaging.EventHubs.Processor;
using Shuttle.Core.Contract;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubProcessErrorEventArgs(ITransport transport, ProcessErrorEventArgs processErrorEventArgs)
{
    public ITransport Transport { get; } = Guard.AgainstNull(transport);
    public ProcessErrorEventArgs ProcessErrorEventArgs { get; } = Guard.AgainstNull(processErrorEventArgs);
}