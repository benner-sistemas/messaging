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
        private static QueueName _queueName = new QueueName($"{Environment.MachineName}-retryteste01");
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
            QueueName = _queueName.Default,
            RetryIntervalInMilliseconds = 10,
            RetryLimit = 2,
        };

        private static EnterpriseIntegrationConsumerMock _consumer = new EnterpriseIntegrationConsumerMock(_settings);

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas_com_rabbitMQ()
        {
            // garante que fila existe
            CreateQueue(_queueName.Default);
            CreateQueue(_queueName.Dead);
            CreateQueue(_queueName.Retry);
            CreateQueue(_queueName.Invalid);

            // garante que está vazia
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

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

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));
        }

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas_com_rabbitMQ()
        {
            // garante que fila existe
            CreateQueue(_queueName.Default);
            CreateQueue(_queueName.Dead);
            CreateQueue(_queueName.Retry);
            CreateQueue(_queueName.Invalid);

            // garante que está vazia
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

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

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            PurgeQueue(_queueName.Retry);
            PurgeQueue(_queueName.Invalid);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            Assert.AreEqual(0, GetQueueSize(_queueName.Retry));
            Assert.AreEqual(0, GetQueueSize(_queueName.Invalid));
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
