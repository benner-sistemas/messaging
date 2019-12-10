using Benner.Listener;
using Benner.Messaging;
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

        public void OnDeadMessage(object message)
        {
            ++OnDeadMessageCount;
        }

        public void OnInvalidMessage(object message)
        {
            ++OnInvalidMessageCount;
        }

        public void OnMessage(object message)
        {
            ++OnMessageCount;

            var messageAsString = message as string;
            if (messageAsString.Equals("emitir-excecao"))
                throw new Exception(messageAsString);
            if (messageAsString.Equals("emitir-excecao-mensagem-invalida"))
                throw new InvalidMessageException();
        }
    }
}
