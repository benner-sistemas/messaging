namespace Benner.Listener
{
    public interface IEnterpriseIntegrationSettings
    {
        string QueueName { get; }
        int RetryLimit { get; }
        long RetryIntervalInMilliseconds { get; }
    }
}