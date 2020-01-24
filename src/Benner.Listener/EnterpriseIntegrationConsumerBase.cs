using Benner.Messaging.Configuration;
using System;

namespace Benner.Listener
{
    public abstract class EnterpriseIntegrationConsumerBase : IEnterpriseIntegrationConsumer
    {
        public abstract IEnterpriseIntegrationSettings Settings { get; }

        public abstract void OnDeadMessage(string message, Exception exception);

        public abstract void OnInvalidMessage(string message, InvalidMessageException exception);

        public abstract void OnMessage(string message);

        protected virtual T DeserializeMessage<T>(string message)
        {
            return JsonParser.Deserialize<T>(message);
        }
    }
}