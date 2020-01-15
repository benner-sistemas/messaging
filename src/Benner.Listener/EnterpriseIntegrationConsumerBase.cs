using Benner.Messaging;

namespace Benner.Listener
{
    public abstract class EnterpriseIntegrationConsumerBase : IEnterpriseIntegrationConsumer
    {
        public abstract IEnterpriseIntegrationSettings Settings { get; }

        public abstract void OnDeadMessage(string message);

        public abstract void OnInvalidMessage(string message);

        public abstract void OnMessage(string message);

        protected virtual T DeserializeMessage<T>(string message)
        {
            return Utils.DeserializeObject<T>(message);
        }
    }
}