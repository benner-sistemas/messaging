﻿namespace Benner.Enterprise.Integration.Messaging
{
    public interface IEnterpriseIntegrationSettings
    {
        string QueueName { get; }
        int RetryLimit { get; }
        long RetryIntervalInMilliseconds { get; }
    }
}