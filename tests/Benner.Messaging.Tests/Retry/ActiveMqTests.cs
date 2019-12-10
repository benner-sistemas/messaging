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
        private readonly QueueName _queueName = new QueueName($"{Environment.MachineName}-retryteste01");
        private readonly ConnectionFactory factory = new ConnectionFactory("tcp://bnu-vtec001:61616");
        private readonly MessagingConfig config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" }
            }).Create();
        private readonly EnterpriseIntegrationConsumerMock consumer = new EnterpriseIntegrationConsumerMock(null);

        [TestMethod]
        public void Testa_retentativa_activemq()
        {
            try
            {
                var guid = Guid.NewGuid().ToString();

                var message = new EnterpriseIntegrationMessage()
                {
                    Body = "emitir-excecao",
                    MessageID = Guid.NewGuid().ToString()
                };

                var listener = new EnterpriseIntegrationListener(config, consumer);
                var producer = new Messaging(config);

                producer.EnqueueMessage(_queueName.Default, message);

                Assert.AreEqual(1, GetQueueSize(_queueName.Default));

                listener.Start();

                Thread.Sleep(1000);

                var received = Messaging.Dequeue<EnterpriseIntegrationMessage>(_queueName.Dead, config);

                Assert.AreEqual(0, GetQueueSize(_queueName.Dead));
                Assert.AreEqual(message.MessageID, received.MessageID);
                Assert.AreEqual(0, GetQueueSize(_queueName.Default));
                Assert.AreEqual(1, consumer.OnMessageCount);
            }
            catch (Exception e)
            {
                using (Connection conn = factory.CreateConnection() as Connection)
                {
                    using (ISession session = conn.CreateSession())
                    {
                        session.DeleteQueue(_queueName.Default);
                        session.DeleteQueue(_queueName.Dead);
                        session.DeleteQueue(_queueName.Retry);
                        session.DeleteQueue(_queueName.Invalid);
                    }
                }
                throw new Exception(e.InnerException.ToString());
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