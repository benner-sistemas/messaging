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
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(queueName);
                    session.DeleteQueue(errorQueueName);
                }
            }

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

                        // garantir que a mensagem lida, é a mensagem enviada
                        Assert.AreEqual(message, e.AsString);

                        // simulando um erro aleatorio durante o consumo da mensagem
                        throw new Exception(message);
                    });
                    Thread.Sleep(1_000);
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
            var guid = Guid.NewGuid().ToString();
            var message = new Messaging();
            var producer = new Messaging(config);
            producer.EnqueueMessage(queueName, message);

            // antes de consumir, garantir que o tamanho da fila é 1
            Assert.AreEqual(1, GetQueueSize(queueName));

            // limpar a fila de erro
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(errorQueueName);
                    session.DeleteQueue(queueName);
                }
            }
            //  garantir que a fila de erro está zerada
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            try
            {
                using (var client = new Messaging(config))
                {
                    client.StartListening(queueName, (e) =>
                    {
                        // garantir que a mensagem lida, é a mensagem enviada
                        Assert.AreEqual(message.ToString(), guid);

                        // retornando true, tem que tirar da fila principal, e não gerar erro
                        return true;
                    });
                }
            }
            catch { }
            // depois de consumir, com erro, garantir que o tamanho da fila normal é 0
            Assert.AreEqual(0, GetQueueSize(queueName));

            // depois de consumir, com erro, garantir que o tamanho da fila de erro é 1
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_sem_executar_o_consume_da_fila_no_activeMq()
        {
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(errorQueueName);
                    session.DeleteQueue(queueName);
                }
            }

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

                    // retornando false, para não disparar o acknowledge
                    return false;
                });
            }

            Assert.IsFalse(consumerFired);
            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }
        [TestMethod]
        public void Testa_o_not_acknowledge_no_metodo_de_recebimento_executanso_o_consume_da_fila_no_activeMq()
        {
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(errorQueueName);
                    session.DeleteQueue(queueName);
                }
            }

            var message = Guid.NewGuid().ToString();
            var producer = new Messaging(config);
            producer.EnqueueMessage(queueName, message);

            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
            var consumerFired = false;
            using (var client = new Messaging(config))
            {
                client.StartListening(queueName, (e) =>
                {
                    consumerFired = true;
                    
                    Assert.IsTrue(e.AsString == message);

                    // retornando false, para não disparar o acknowledge
                    return false;
                });
                Thread.Sleep(1_000);
            }

            Assert.IsTrue(consumerFired);
            Assert.AreEqual(1, GetQueueSize(queueName));
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_garantia_de_recebimento_da_fila_no_activeMq_sem_using()
        {
            var guid = Guid.NewGuid().ToString();
            var message = new Messaging();
            var producer = new Messaging(config);

            // limpar a fila de erro
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(errorQueueName);
                    session.DeleteQueue(queueName);
                }
            }

            producer.EnqueueMessage(queueName, message);

            // antes de consumir, garantir que o tamanho da fila é 1
            Assert.AreEqual(1, GetQueueSize(queueName));
            try
            {
                var receivedMessage = Messaging.Dequeue(queueName, config);
            }
            catch { }
            // depois de consumir, com erro, garantir que o tamanho da fila ainda é 1
            Assert.AreEqual(0, GetQueueSize(queueName));

            // a fila de erro também precisa estar zerada
            Assert.AreEqual(0, GetQueueSize(errorQueueName));
        }

        [TestMethod]
        public void Testa_garantia_de_envio_para_fila_no_activeMq()
        {
            var guid = Guid.NewGuid().ToString();

            // antes de enviar, garantir que a fila está vazia
            using (Connection conn = factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.DeleteQueue(errorQueueName);
                    session.DeleteQueue(queueName);
                }
            }
            try
            {

                using (var client = new Messaging(config))
                {
                    bool exceptionWasThrow = false;
                    try
                    {
                        client.EnqueueMessage(queueName, new { id = guid });
                    }
                    catch
                    {
                        exceptionWasThrow = true;
                    }
                    // garantir que exceptionWasThrow é false
                    Assert.IsFalse(exceptionWasThrow);
                    // depois do envio, garantir que a fila tem 1
                    Assert.AreEqual(1, GetQueueSize(queueName));
                    // disparar uma exceção antes do dispose
                    throw new Exception(guid);
                }
            }
            catch (Exception exception)
            {
                // garantir que a execao qui é a mesma do throw ali em cima
                Assert.AreEqual(guid, exception.Message);
            }
            // depois do envio, garantir que a fila tem 1
            Assert.AreEqual(1, GetQueueSize(queueName));
            // ler da fila e garantir que a mensagem da fila é exatamente aquela que foi enviada
            var received = Messaging.Dequeue(queueName, config);
            Assert.IsTrue(received.Contains(guid));
        }

        public void Testa_garantia_de_envio_para_fila_no_activeMq_sem_using()
        {
            var guid = Guid.NewGuid().ToString();
            try
            {
                // antes de enviar, garantir que a fila está vazia
                var client = new Messaging(config);

                bool exceptionWasThrow = false;
                try
                {
                    client.EnqueueMessage(queueName, new { id = guid });
                }
                catch
                {
                    exceptionWasThrow = true;
                }
                // garantir que exceptionWasThrow é false
                Assert.IsFalse(exceptionWasThrow);
                // depois do envio, garantir que a fila tem 1
                Assert.AreEqual(1, GetQueueSize(queueName));
                // disparar uma exceção antes do dispose
                throw new Exception(guid);
            }
            catch (Exception exception)
            {
                // garantir que a execao qui é a mesma do throw ali em cima
                Assert.AreEqual(guid, exception.Message);
            }
            // depois do envio, garantir que a fila tem 1
            Assert.AreEqual(1, GetQueueSize(queueName));
            // ler da fila e garantir que a mensagem da fila é exatamente aquela que foi enviada
            var received = Messaging.Dequeue(queueName, config);
            Assert.IsTrue(received.Contains(guid));
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