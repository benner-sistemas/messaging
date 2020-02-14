﻿namespace Benner.Enterprise.Integration.Messaging
{
    public class EnterpriseIntegrationSettings : IEnterpriseIntegrationSettings
    {
        public string QueueName { get; set; }

        public int RetryLimit { get; set; }

        public long RetryIntervalInMilliseconds { get; set; }
    }
}