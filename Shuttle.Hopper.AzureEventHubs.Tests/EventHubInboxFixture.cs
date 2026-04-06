using NUnit.Framework;
using Shuttle.Hopper.Testing;

namespace Shuttle.Hopper.AzureEventHubs.Tests;

public class EventHubInboxFixture : InboxFixture
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task Should_be_able_handle_errors_async(bool hasErrorQueue)
    {
        await TestInboxErrorAsync(AzureEventHubsConfiguration.GetServiceCollection(), "azureeh://azure/{0}", hasErrorQueue, TimeSpan.FromMinutes(1));
    }

    [Test]
    public async Task Should_be_able_to_process_queue_timeously_async()
    {
        await TestInboxThroughputAsync(AzureEventHubsConfiguration.GetServiceCollection(), "azureeh://azure/{0}", 2000, 5, TimeSpan.FromMinutes(2));
    }
}