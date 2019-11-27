using Benner.Listener;
using System;

namespace Benner.Consumer.Core
{
    public class ContabilizacaoConsumer : IEnterpriseIntegrationConsumer
    {
        public IEnterpriseIntegrationSettings Settings => new EnterpriseIntegrationSettings()
        {
            QueueName = "contabilizacao",
            RetryIntervalInMilliseconds = 1000 * 60,
            RetryLimit = 10,
        };

        public void OnDeadMessage(object message)
        {
            throw new NotImplementedException();
        }

        public void OnInvalidMessage(object message)
        {
            throw new NotImplementedException();
        }

        public void OnMessage(object message)
        {
            throw new NotImplementedException();
        }
    }
}
