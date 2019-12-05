using Benner.Listener;
using Benner.Messaging;
using Benner.Messaging.Retry.Tests.Mocks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Benner.Retry.Tests.MockMQ
{
    public class EnterpriseIntegrationConsumerMock : IEnterpriseIntegrationConsumer
    {
        public IEnterpriseIntegrationSettings Settings => new SettingsMock();

        public void OnDeadMessage(object message)
        {
            throw new NotImplementedException();
        }

        public void OnInvalidMessage(object message)
        {
        }

        public void OnMessage(object message)
        {
            var mensagem = message as EnterpriseIntegrationMessage;
            if (mensagem.Body.ToString() == "emitir-excecao")
                throw new Exception();
        }
    }
}
