using Benner.Listener;
using System;

namespace Benner.Retry.Tests.MockMQ
{
    public class EnterpriseIntegrationConsumerMock : IEnterpriseIntegrationConsumer
    {
        public EnterpriseIntegrationConsumerMock(IEnterpriseIntegrationSettings settings) => this.Settings = settings;

        public IEnterpriseIntegrationSettings Settings { get; set; }

        public int OnMessageCount { get; internal set; }
        public int OnDeadMessageCount { get; internal set; }
        public int OnInvalidMessageCount { get; internal set; }

        public void OnDeadMessage(string message, Exception exception)
        {
            ++OnDeadMessageCount;
        }

        public void OnInvalidMessage(string message, InvalidMessageException exception)
        {
            ++OnInvalidMessageCount;
        }

        public void OnMessage(string message)
        {
            ++OnMessageCount;

            if (message.Equals("emitir-excecao"))
                throw new Exception(message);
            if (message.Equals("emitir-excecao-mensagem-invalida"))
                throw new InvalidMessageException();
        }
    }
}
