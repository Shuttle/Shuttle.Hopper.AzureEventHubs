using Microsoft.Extensions.Options;

namespace Shuttle.Hopper.AzureEventHubs;

public class EventHubOptionsValidator : IValidateOptions<EventHubOptions>
{
    public ValidateOptionsResult Validate(string? name, EventHubOptions options)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ValidateOptionsResult.Fail(Hopper.Resources.TransportConfigurationNameException);
        }

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(string.Format(Hopper.Resources.TransportConfigurationItemException, name, nameof(options.ConnectionString)));
        }

        if (options.ProcessEvents)
        {
            if (string.IsNullOrWhiteSpace(options.BlobStorageConnectionString))
            {
                return ValidateOptionsResult.Fail(string.Format(Hopper.Resources.TransportConfigurationItemException, name, nameof(options.BlobStorageConnectionString)));
            }

            if (string.IsNullOrWhiteSpace(options.BlobContainerName))
            {
                return ValidateOptionsResult.Fail(string.Format(Hopper.Resources.TransportConfigurationItemException, name, nameof(options.BlobContainerName)));
            }

            if (string.IsNullOrWhiteSpace(options.ConsumerGroup))
            {
                return ValidateOptionsResult.Fail(string.Format(Hopper.Resources.TransportConfigurationItemException, name, nameof(options.ConsumerGroup)));
            }
        }

        if (options.CheckpointInterval < 1)
        {
            return ValidateOptionsResult.Fail(Resources.InvalidCheckpointIntervalException);
        }

        return ValidateOptionsResult.Success;
    }
}