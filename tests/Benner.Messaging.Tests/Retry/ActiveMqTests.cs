using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Benner.Listener;
using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class ActiveMqTests
    {
        private static QueueName _queueName = new QueueName($"{Environment.MachineName}-retryteste01");

        private readonly ConnectionFactory _factory = new ConnectionFactory("tcp://bnu-vtec001:61616");

        private readonly MessagingConfig _config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" },
                {"Port", "61616" }
            }).Create();

        private static IEnterpriseIntegrationSettings _settings = new EnterpriseIntegrationSettings
        {
            QueueName = _queueName.Default,
            RetryIntervalInMilliseconds = 10,
            RetryLimit = 2,
        };

        private static EnterpriseIntegrationConsumerMock _consumer = new EnterpriseIntegrationConsumerMock(_settings);


        [TestMethod]
        public void EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas_com_activeMQ()
        {
            // garante que fila existe
            CreateQueue(_queueName.Default);
            CreateQueue(_queueName.Dead);
            CreateQueue(_queueName.Retry);
            CreateQueue(_queueName.Invalid);

            // garante que está vazia
            TryDeleteQueue(_queueName.Default);
            TryDeleteQueue(_queueName.Dead);
            TryDeleteQueue(_queueName.Retry);
            TryDeleteQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            //zera os contadores de mensagens
            _consumer.OnMessageCount = 0;
            _consumer.OnInvalidMessageCount = 0;
            _consumer.OnDeadMessageCount = 0;
            using (var producer = new Messaging(_config))
            {
                for (int i = 0; i < 2; i++)
                {
                    producer.EnqueueMessage(_queueName.Default, new EnterpriseIntegrationMessage()
                    {
                        Body = "emitir-excecao",
                        MessageID = Guid.NewGuid().ToString()
                    });
                }
            }

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            using (var listener = new EnterpriseIntegrationListener(_config, _consumer))
            {
                listener.Start();

                for (int index = 0; index < 100 && _consumer.OnDeadMessageCount < 4; ++index)
                    Thread.Sleep(10);

                //TODO: testar o tamanho da fila de retentativas, de alguma forma
            }

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(2, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            // garantir a quantidade de retentativas

            Assert.AreEqual(4, _consumer.OnMessageCount);
            Assert.AreEqual(2, _consumer.OnDeadMessageCount);
            Assert.AreEqual(0, _consumer.OnInvalidMessageCount);

            TryDeleteQueue(_queueName.Default);
            TryDeleteQueue(_queueName.Dead);
            TryDeleteQueue(_queueName.Retry);
            TryDeleteQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, _consumer.OnInvalidMessageCount);
        }

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas_com_activeMQ()
        {
            // garante que fila existe
            CreateQueue(_queueName.Default);
            CreateQueue(_queueName.Dead);
            CreateQueue(_queueName.Retry);
            CreateQueue(_queueName.Invalid);

            // garante que está vazia
            TryDeleteQueue(_queueName.Default);
            TryDeleteQueue(_queueName.Dead);
            TryDeleteQueue(_queueName.Retry);
            TryDeleteQueue(_queueName.Invalid);

            //zera os contadores de mensagens
            _consumer.OnMessageCount = 0;
            _consumer.OnInvalidMessageCount = 0;
            _consumer.OnDeadMessageCount = 0;

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            using (var producer = new Messaging(_config))
            {
                for (int i = 0; i < 2; i++)
                {
                    producer.EnqueueMessage(_queueName.Default, new EnterpriseIntegrationMessage()
                    {
                        Body = "emitir-excecao-mensagem-invalida",
                        MessageID = Guid.NewGuid().ToString()
                    });
                }
            }

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));

            using (var listener = new EnterpriseIntegrationListener(_config, _consumer))
            {
                listener.Start();

                for (int index = 0; index < 100 && _consumer.OnInvalidMessageCount < 2; ++index)
                    Thread.Sleep(10);

                //TODO: testar o tamanho da fila de retentativas, de alguma forma
            }

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(2, GetQueueSize(_queueName.Invalid));

            // garantir a quantidade de retentativas
            Assert.AreEqual(2, _consumer.OnMessageCount);
            Assert.AreEqual(0, _consumer.OnDeadMessageCount);
            Assert.AreEqual(2, _consumer.OnInvalidMessageCount);



            TryDeleteQueue(_queueName.Default);
            TryDeleteQueue(_queueName.Dead);
            TryDeleteQueue(_queueName.Retry);
            TryDeleteQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));
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