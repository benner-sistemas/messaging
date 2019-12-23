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
        private readonly QueueName _queueName = new QueueName($"{Environment.MachineName}-ackteste01");

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



        [TestMethod]
        public void Envia_mensagem_para_fila_de_contabilizacao()
        {
            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue("contabilizacao", message, config);
        }


        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_rabbitMq()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            var consumerFired = false;
            try
            {
                using (var client = new Messaging(config))
                {
                    client.StartListening(_queueName.Default, (e) =>
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
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(1, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila_no_rabbitMq()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(_queueName.Default, (e) =>
                {
                    consumerFired = true;
                    Assert.AreEqual(message.ToString(), e.AsString);
                    return true;
                });
                for (int index = 0; index < 20 && !consumerFired; ++index)
                    Thread.Sleep(1000);
            }
            Assert.IsTrue(consumerFired);
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_rabbitMq()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                Thread.Sleep(10);
                client.StartListening(_queueName.Default, null);
            }
            Thread.Sleep(10);

            Assert.IsFalse(consumerFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila_no_rabbitMq()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Messaging.Enqueue(_queueName.Default, "Message_A", config);
            Messaging.Enqueue(_queueName.Default, "Message_B", config);

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            var client01 = new Messaging(config);
            var client02 = new Messaging(config);

            client01.StartListening(_queueName.Default, ProcessQueueAorB);
            client02.StartListening(_queueName.Default, ProcessQueueAorB);

            for (int index = 0; index < 200 && !(_consumerFired_A && _consumerFired_B); ++index)
                Thread.Sleep(100);

            client01.Dispose();
            client02.Dispose();

            Assert.IsTrue(_consumerFired_A);
            Assert.IsTrue(_consumerFired_B);
            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));

            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        private bool _consumerFired_A = false;
        private bool _consumerFired_B = false;

        private bool ProcessQueueAorB(MessagingArgs arg)
        {
            if (arg.AsString == "Message_A")
                _consumerFired_A = true;

            if (arg.AsString == "Message_B")
                _consumerFired_B = true;

            return false;
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executanso_o_consume_da_fila_no_rabbitMq()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(_queueName.Default, message, config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(_queueName.Default, (e) =>
                {
                    consumerFired = true;

                    Assert.IsTrue(e.AsString == message);

                    return false;
                });
                for (int index = 0; index < 20 && !consumerFired; ++index)
                    Thread.Sleep(1000);
            }

            Assert.IsTrue(consumerFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_rabbitMq_sem_using()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();


            Messaging.Enqueue(_queueName.Default, message, config);

            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            try
            {
                var receivedMessage = Messaging.Dequeue(_queueName.Default, config);
            }
            catch { }
            Assert.AreEqual(0, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_rabbitMq()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var message = Guid.NewGuid().ToString();

            try
            {
                using (var client = new Messaging(config))
                {
                    bool exceptionWasThrow = false;
                    try
                    {
                        client.EnqueueMessage(_queueName.Default, message);
                    }
                    catch
                    {
                        exceptionWasThrow = true;
                    }
                    Assert.IsFalse(exceptionWasThrow);
                    Assert.AreEqual(1, GetQueueSize(_queueName.Default));
                    throw new Exception(message);
                }
            }
            catch (Exception exception)
            {
                Assert.AreEqual(message, exception.Message);
            }
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            var received = Messaging.Dequeue(_queueName.Default, config);
            Assert.IsTrue(received.Contains(message));
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_rabbitMq_sem_using()
        {
            PurgeQueue(_queueName.Default);
            PurgeQueue(_queueName.Dead);

            var guid = Guid.NewGuid().ToString();
            try
            {
                bool exceptionWasThrow = false;
                try
                {
                    Messaging.Enqueue(_queueName.Default, new { id = guid }, config);
                }
                catch
                {
                    exceptionWasThrow = true;
                }
                Assert.IsFalse(exceptionWasThrow);
                Assert.AreEqual(1, GetQueueSize(_queueName.Default));
                throw new Exception(guid);
            }
            catch (Exception exception)
            {
                Assert.AreEqual(guid, exception.Message);
            }
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            var received = Messaging.Dequeue(_queueName.Default, config);
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
                    }
                    catch
                    { }
                }
            }
        }
    }
}
