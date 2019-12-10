using Benner.Listener;
using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class RabbitMQTests
    {
        private static string _queueName = $"{Environment.MachineName}-test".ToLower();
        private static string _deadQueueName = $"{Environment.MachineName}-test-dead".ToLower();
        private static string _retryQueueName = $"{Environment.MachineName}-test-retry".ToLower();
        private static string _invalidQueueName = $"{Environment.MachineName}-test-invalid".ToLower();

        private static ConnectionFactory _factory = new ConnectionFactory
        {
            HostName = "bnu-vtec012",
            UserName = "guest",
            Port = 5672
        };

        private static MessagingConfig _config = new MessagingConfigBuilder("RabbitMQ", BrokerType.RabbitMQ, new Dictionary<string, string>()
                {
                    {"UserName", "guest"},
                    {"Password", "guest"},
                    {"HostName", "bnu-vtec012"}
                })
                .Create();

        private static IEnterpriseIntegrationSettings _settings = new EnterpriseIntegrationSettings
        {
            QueueName = _queueName,
            RetryIntervalInMilliseconds = 10,
            RetryLimit = 2,
        };

        private static EnterpriseIntegrationConsumerMock _consumer = new EnterpriseIntegrationConsumerMock(_settings);

        [TestMethod]
        public void testa_retentativa_rabbitmq()
        {
            // garante que fila existe
            CreateQueue(_queueName);
            CreateQueue(_deadQueueName);
            CreateQueue(_retryQueueName);
            CreateQueue(_invalidQueueName);

            // garante que está vazia
            PurgeQueue(_queueName);
            PurgeQueue(_deadQueueName);
            PurgeQueue(_retryQueueName);
            PurgeQueue(_invalidQueueName);

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

            PurgeQueue(_queueName);
            PurgeQueue(_deadQueueName);
            PurgeQueue(_retryQueueName);
            PurgeQueue(_invalidQueueName);

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
            PurgeQueue(_queueName);
            PurgeQueue(_deadQueueName);
            PurgeQueue(_retryQueueName);
            PurgeQueue(_invalidQueueName);

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

            PurgeQueue(_queueName);
            PurgeQueue(_deadQueueName);
            PurgeQueue(_retryQueueName);
            PurgeQueue(_invalidQueueName);

            Assert.AreEqual(0, GetQueueSize(_queueName));
            Assert.AreEqual(0, GetQueueSize(_deadQueueName));
            Assert.AreEqual(0, GetQueueSize(_retryQueueName));
            Assert.AreEqual(0, GetQueueSize(_invalidQueueName));
        }

        private void PurgeQueue(string queueName)
        {
            using (var conn = _factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueuePurge(queueName);
                }
            }
        }

        private void CreateQueue(string queueName)
        {
            using (var conn = _factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                }
            }
        }
        private int GetQueueSize(string queueName)
        {
            using (var conn = _factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    return (int)channel.MessageCount(queueName);
                }
            }
        }
    }
}
