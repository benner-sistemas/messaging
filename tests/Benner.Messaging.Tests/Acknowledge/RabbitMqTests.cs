using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Benner.Messaging.Tests.Acknowledge
{

    [TestClass]
    public class RabbitMqTests
    {
        private readonly string queueName = $"{Environment.MachineName}-teste-ack".ToLower();
        private readonly string errorQueueName = $"{Environment.MachineName}-teste-ack-error".ToLower();
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
        public void Testa_garantia_de_recebimento_da_fila_no_rabbitMq()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            var consumerFired = false;
            try
            {
                using (var client = new Messaging(config))
                {
                    client.StartListening(queueName, (e) =>
                    {
                        consumerFired = true;

                        Assert.AreEqual(message, e.AsString);

                        throw new Exception(message);
                    });

                    for (int index = 0; index < 20 && !consumerFired; ++index)
                        Thread.Sleep(1000);
                }
            }
            catch (Exception exception)
            {
                Assert.AreEqual(message, exception.Message);
            }

            Assert.IsTrue(consumerFired);
            Assert.AreEqual(0, GetQueueSize(queueName));
            Assert.AreEqual(1, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila_no_rabbitMq()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));

            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(queueName, (e) =>
                {
                    consumerFired = true;
                    Assert.AreEqual(message.ToString(), e.AsString);
                    return true;
                });
                for (int index = 0; index < 20 && !consumerFired; ++index)
                    Thread.Sleep(1000);
            }
            Assert.IsTrue(consumerFired);
            Assert.AreEqual(0, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_rabbitMq()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(queueName, (e) =>
                {
                    consumerFired = true;

                    Assert.AreEqual(message, e.AsString);

                    return false;
                });
            }

            Assert.IsFalse(consumerFired);
            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila_no_rabbitMq()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            Messaging.Enqueue(queueName, "Message_A", config);
            Messaging.Enqueue(queueName, "Message_B", config);

            Assert.AreEqual(2, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            var consumerFired_A = false;
            var consumerFired_B = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(queueName, (e) =>
                {
                    if (e.AsString == "Message_A")
                        consumerFired_A = true;

                    if (e.AsString == "Message_B")
                        consumerFired_B = true;

                    return false;
                });
                for (int index = 0; index < 20 && !(consumerFired_A && consumerFired_B); ++index)
                    Thread.Sleep(1000);
            }

            Assert.IsTrue(consumerFired_A);
            Assert.IsTrue(consumerFired_B);
            Assert.AreEqual(2, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executanso_o_consume_da_fila_no_rabbitMq()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(queueName, (e) =>
                {
                    consumerFired = true;

                    Assert.IsTrue(e.AsString == message);

                    return false;
                });
                for (int index = 0; index < 20 && !consumerFired; ++index)
                    Thread.Sleep(1000);
            }

            Assert.IsTrue(consumerFired);
            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_rabbitMq_sem_using()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var message = Guid.NewGuid().ToString();


            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(1, GetQueueSize(queueName));
            try
            {
                var receivedMessage = Messaging.Dequeue(queueName, config);
            }
            catch { }
            Assert.AreEqual(0, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_rabbitMq()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var message = Guid.NewGuid().ToString();

            try
            {
                using (var client = new Messaging(config))
                {
                    bool exceptionWasThrow = false;
                    try
                    {
                        client.EnqueueMessage(queueName, message);
                    }
                    catch
                    {
                        exceptionWasThrow = true;
                    }
                    Assert.IsFalse(exceptionWasThrow);
                    Assert.AreEqual(1, GetQueueSize(queueName));
                    throw new Exception(message);
                }
            }
            catch (Exception exception)
            {
                Assert.AreEqual(message, exception.Message);
            }
            Assert.AreEqual(1, GetQueueSize(queueName));
            var received = Messaging.Dequeue(queueName, config);
            Assert.IsTrue(received.Contains(message));
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_rabbitMq_sem_using()
        {
            PurgeQueue(queueName);
            PurgeQueue(errorQueueName);

            var guid = Guid.NewGuid().ToString();
            try
            {
                bool exceptionWasThrow = false;
                try
                {
                    Messaging.Enqueue(queueName, new { id = guid }, config);
                }
                catch
                {
                    exceptionWasThrow = true;
                }
                Assert.IsFalse(exceptionWasThrow);
                Assert.AreEqual(1, GetQueueSize(queueName));
                throw new Exception(guid);
            }
            catch (Exception exception)
            {
                Assert.AreEqual(guid, exception.Message);
            }
            Assert.AreEqual(1, GetQueueSize(queueName));
            var received = Messaging.Dequeue(queueName, config);
            Assert.IsTrue(received.Contains(guid));
        }

        private int GetQueueSize(string fila)
        {
            uint result = 0;
            try
            {
                using (var conn = factory.CreateConnection())
                {
                    using (var channel = conn.CreateModel())
                    {
                        result = channel.MessageCount(fila);
                    }
                }
            }
            catch
            { }
            return Convert.ToInt32(result);
        }

        private void PurgeQueue(string queueName)
        {

            using (var conn = factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    try
                    {
                        channel.QueuePurge(queueName);
                        channel.QueuePurge(queueName + "-retry");
                        channel.QueuePurge(queueName + "-dead");
                    }
                    catch
                    { }
                }
            }
        }
    }
}
