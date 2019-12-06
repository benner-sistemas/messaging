using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Benner.Retry.Tests.MockMQ;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Benner.Messaging.Tests.Acknonledge
{

    [TestClass]
    public class ActiveMqTests
    {
        private readonly ConnectionFactory factory = new ConnectionFactory("tcp://bnu-vtec001:61616");
        private readonly MessagingConfig config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" }
            }).Create();

        private readonly string queueName = $"{Environment.MachineName}-teste-ack".ToLower();
        private readonly string errorQueueName = $"{Environment.MachineName}-teste-ack-error".ToLower();

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq()
        {
            DeleteQueues();
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
        public void Testa_o_acknowledge_no_metodo_de_recebimento_da_fila_no_activeMq()
        {
            DeleteQueues();
            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            try
            {
                using (var client = new Messaging(config))
                {
                    client.StartListening(queueName, (e) =>
                    {
                        Assert.AreEqual(message.ToString(), e.AsString);
                        return true;
                    });
                }
            }
            catch { }
            Assert.AreEqual(0, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_activeMq()
        {
            DeleteQueues();
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
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_com_duas_mensagens_sem_executar_o_consume_da_fila_no_activeMq()
        {
            DeleteQueues();
            var message = Guid.NewGuid().ToString();
            Messaging.Enqueue(queueName, message, config);
            Messaging.Enqueue(queueName, message, config);

            Assert.AreEqual(2, GetQueueSize(queueName));
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
            Assert.AreEqual(2, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executanso_o_consume_da_fila_no_activeMq()
        {
            DeleteQueues();
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
            }

            Assert.IsTrue(consumerFired);
            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq_sem_using()
        {
            DeleteQueues();
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
        public void Testa_garantia_de_envio_para_fila_no_activeMq_sem_using()
        {
            DeleteQueues();
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

        private void DeleteQueues()
        {
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(queueName);
                    session.DeleteQueue(errorQueueName);
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