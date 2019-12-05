using System;
using System.Collections.Generic;
using System.Threading;
using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class RabbitMqTests
    {
        private readonly string queueName = "nathank-teste";
        private readonly ConnectionFactory factory = new ConnectionFactory
        {
            HostName = "bnu-vtec012",
            UserName = "guest",
            Port = 5672
        };

        private readonly MessagingConfig config = new MessagingConfigBuilder("RabbitMQ", BrokerType.RabbitMQ, new Dictionary<string, string>()
                {
                    {"UserName", "guest"},
                    {"Password", "guest"},
                    {"HostName", "bnu-vtec012"}
                })
                .Create();

        private readonly EnterpriseIntegrationConsumerMock consumer = new EnterpriseIntegrationConsumerMock();

        [TestMethod]
        public void Deve_zerar_fila_principal_e_realizar_retentativa()
        {
            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    try
                    {
                        var producer = new Messaging(config);
                        for (int i = 0; i < 2; i++)
                        {
                            producer.EnqueueMessage(queueName, new EnterpriseIntegrationMessage()
                            {
                                Body = "emitir-excecao",
                                MessageID = Guid.NewGuid().ToString(),
                            });
                        }

                        Assert.AreEqual(Convert.ToUInt32(2), channel.MessageCount(queueName));

                        var listener = new EnterpriseIntegrationListenerMock(config, consumer);
                        listener.Start();

                        Thread.Sleep(1000);

                        Assert.AreEqual(Convert.ToUInt32(0), channel.MessageCount(queueName));

                        Assert.AreEqual(Convert.ToUInt32(2), channel.MessageCount(queueName + "-dead"));
                        Assert.AreEqual(2, listener.GetCountRetry());
                        PurgeQueue(queueName);
                        listener.SetCountRetry(0);
                        conn.Close();
                        channel.Close();
                    }
                    finally
                    {
                        PurgeQueue(queueName);
                        conn.Close();
                        channel.Close();
                    }
                }
            }
        }

        [TestMethod]
        public void Testa_Purge_Queues()
        {
            var producer = new Messaging(config);

            for (int i = 0; i < 2; i++)
            {
                producer.EnqueueMessage(queueName, new EnterpriseIntegrationMessage()
                {
                    Body = "emitir-excecao",
                    MessageID = Guid.NewGuid().ToString(),
                });
            }

            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    PurgeQueue(queueName);
                    Assert.AreEqual(Convert.ToUInt32(0), channel.MessageCount(queueName));
                    Assert.AreEqual(Convert.ToUInt32(0), channel.MessageCount(queueName + "-dead"));
                    Assert.AreEqual(Convert.ToUInt32(0), channel.MessageCount(queueName + "-retry"));
                }
            }
        }


        private void PurgeQueue(string queueName)
        {

            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueuePurge(queueName);
                    channel.QueuePurge(queueName + "-retry");
                    channel.QueuePurge(queueName + "-dead");
                }
            }
        }
    }
}
