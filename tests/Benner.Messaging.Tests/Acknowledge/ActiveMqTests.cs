using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Benner.Messaging.Tests.Acknowledge
{

    [TestClass]
    public class ActiveMqTests
    {
        private readonly QueueName _queueName = new QueueName($"{Environment.MachineName}-ackteste01");

        private readonly ConnectionFactory factory = new ConnectionFactory("tcp://bnu-vtec001:61616");
        private readonly MessagingConfig config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" }
            }).Create();


        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq()
        {
            DeleteQueues();
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
        public void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila_no_activeMq()
        {
            DeleteQueues();
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
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_activeMq()
        {
            DeleteQueues();
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

            Assert.IsFalse(consumerFired);
            Assert.AreEqual(1, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila_no_activeMq()
        {
            DeleteQueues();

            Messaging.Enqueue(_queueName.Default, "Message_A", config);
            Messaging.Enqueue(_queueName.Default, "Message_B", config);

            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
            var consumerFired_A = false;
            var consumerFired_B = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(_queueName.Default, (e) =>
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
            Assert.AreEqual(2, GetQueueSize(_queueName.Default));
            Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executanso_o_consume_da_fila_no_activeMq()
        {
            DeleteQueues();
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
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq_sem_using()
        {
            DeleteQueues();
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
        public void Testa_garantia_de_envio_para_fila_no_activeMq()
        {
            DeleteQueues();
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
        public void Testa_garantia_de_envio_para_fila_no_activeMq_sem_using()
        {
            DeleteQueues();
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

        private void DeleteQueues()
        {
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(_queueName.Default);
                    session.DeleteQueue(_queueName.Dead);
                }
            }
        }

        public int GetQueueSize(string fila)
        {
            int count = 0;
            using (Connection conn = factory.CreateConnection() as Connection)
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