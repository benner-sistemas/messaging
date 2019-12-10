using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Benner.Messaging;
using System.Collections.Generic;
using System.Threading;
using Apache.NMS;
using System.Collections;
using Apache.NMS.ActiveMQ;
using Benner.Retry.Tests.MockMQ;
using Benner.Listener;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class ActiveMqTests
    {
        private static string _queueName = $"{Environment.MachineName}-test".ToLower();
        private static string _deadQueueName = $"{Environment.MachineName}-test-dead".ToLower();
        private static string _retryQueueName = $"{Environment.MachineName}-test-retry".ToLower();
        private static string _invalidQueueName = $"{Environment.MachineName}-test-invalid".ToLower();

        private readonly ConnectionFactory _factory = new ConnectionFactory("tcp://bnu-vtec001:61616");

        private readonly MessagingConfig _config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" }
            }).Create();

        private static IEnterpriseIntegrationSettings _settings = new EnterpriseIntegrationSettings
        {
            QueueName = _queueName,
            RetryIntervalInMilliseconds = 10,
            RetryLimit = 2,
        };

        private static EnterpriseIntegrationConsumerMock _consumer = new EnterpriseIntegrationConsumerMock(_settings);


        [TestMethod]
        public void Testa_retentativas_activemq()
        {
            // garante que fila existe
            CreateQueue(_queueName);
            CreateQueue(_deadQueueName);
            CreateQueue(_retryQueueName);
            CreateQueue(_invalidQueueName);

            // garante que está vazia
            TryDeleteQueue(_queueName);
            TryDeleteQueue(_deadQueueName);
            TryDeleteQueue(_retryQueueName);
            TryDeleteQueue(_invalidQueueName);

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));

            //zera os contadores de mensagens
            _consumer.OnMessageCount = 0;
            _consumer.OnInvalidMessageCount = 0;
            _consumer.OnDeadMessageCount = 0;
            using (var producer = new Messaging(_config))
            {
                for (int i = 0; i < 2; i++)
                {
                    producer.EnqueueMessage(_queueName, new EnterpriseIntegrationMessage()
                    {
                        Body = "emitir-excecao",
                        MessageID = Guid.NewGuid().ToString()
                    });
                }
            }

            Assert.AreEqual(2, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));

            using (var listener = new EnterpriseIntegrationListener(_config, _consumer))
            {
                listener.Start();
                Thread.Sleep(1000);

                //TODO: testar o tamanho da fila de retentativas, de alguma forma
            }

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(2, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));

            // garantir a quantidade de retentativas

            Assert.AreEqual(4, _consumer.OnMessageCount);
            Assert.AreEqual(2, _consumer.OnDeadMessageCount);
            Assert.AreEqual(0, _consumer.OnInvalidMessageCount);

            TryDeleteQueue(_queueName);
            TryDeleteQueue(_deadQueueName);
            TryDeleteQueue(_retryQueueName);
            TryDeleteQueue(_invalidQueueName);

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, _consumer.OnInvalidMessageCount);
        }

        [TestMethod]
        public void testa_envio_de_mensagens_invalidas()
        {
            // garante que fila existe
            CreateQueue(_queueName);
            CreateQueue(_deadQueueName);
            CreateQueue(_retryQueueName);
            CreateQueue(_invalidQueueName);

            // garante que está vazia
            TryDeleteQueue(_queueName);
            TryDeleteQueue(_deadQueueName);
            TryDeleteQueue(_retryQueueName);
            TryDeleteQueue(_invalidQueueName);

            //zera os contadores de mensagens
            _consumer.OnMessageCount = 0;
            _consumer.OnInvalidMessageCount = 0;
            _consumer.OnDeadMessageCount = 0;

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));

            using (var producer = new Messaging(_config))
            {
                for (int i = 0; i < 2; i++)
                {
                    producer.EnqueueMessage(_queueName, new EnterpriseIntegrationMessage()
                    {
                        Body = "emitir-excecao-mensagem-invalida",
                        MessageID = Guid.NewGuid().ToString()
                    });
                }
            }

            Assert.AreEqual(2, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));

            using (var listener = new EnterpriseIntegrationListener(_config, _consumer))
            {
                listener.Start();
                Thread.Sleep(1000);

                //TODO: testar o tamanho da fila de retentativas, de alguma forma
            }

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(2, GetQueueSize(_invalidQueueName));

            // garantir a quantidade de retentativas
            Assert.AreEqual(2, _consumer.OnMessageCount);
            Assert.AreEqual(0, _consumer.OnDeadMessageCount);
            Assert.AreEqual(2, _consumer.OnInvalidMessageCount);



            TryDeleteQueue(_queueName);
            TryDeleteQueue(_deadQueueName);
            TryDeleteQueue(_retryQueueName);
            TryDeleteQueue(_invalidQueueName);

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));
        }


        internal void CreateQueue(string queueName)
        {
            using (Connection conn = _factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.GetQueue(queueName);
                }
            }
        }

        internal void TryDeleteQueue(string queueName)
        {
            using (Connection connection = _factory.CreateConnection() as Connection)
            using (ISession session = connection.CreateSession())
            {
                IQueue queue = session.GetQueue(queueName);
                try
                {
                    connection.DeleteDestination(queue);
                }
                catch
                { /*ignore errros*/ }
            }
        }

        public int GetQueueSize(string fila)
        {
            int count = 0;
            using (Connection conn = _factory.CreateConnection() as Connection)
            {
                conn.Start();
                using (ISession session = conn.CreateSession())
                {
                    IQueue queue = session.GetQueue(fila);
                    using (IQueueBrowser queueBrowser = session.CreateBrowser(queue))
                    {
                        IEnumerator messages = queueBrowser.GetEnumerator();
                        while (messages.MoveNext())
                        {
                            IMessage message = (IMessage)messages.Current;
                            count++;
                        }
                    }
                }
            }
            return count;
        }
    }
}