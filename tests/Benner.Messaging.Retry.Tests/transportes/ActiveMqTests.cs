using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Benner.Messaging;
using System.Collections.Generic;
using Benner.Retry.Tests.MockMQ;
using System.Threading;
using Apache.NMS;
using System.Collections;
using Apache.NMS.ActiveMQ;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class ActiveMqTests
    {
        private readonly ConnectionFactory factory = new ConnectionFactory("tcp://bnu-vtec001:61616");
        private readonly EnterpriseIntegrationConsumerMock consumer = new EnterpriseIntegrationConsumerMock();
        private readonly MessagingConfig config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" }
            }).Create();
        private readonly string queueName = "fila-teste-nathank";

        [TestMethod]
        public void Testa_retentativa_activemq()
        {
            var guid = Guid.NewGuid().ToString();

            var message = new EnterpriseIntegrationMessage()
            {
                Body = "emitir-excecao",
                MessageID = Guid.NewGuid().ToString()
            };

            var listener = new EnterpriseIntegrationListenerMock(config, consumer);
            var producer = new Messaging(config);


            producer.EnqueueMessage(queueName, message);
            Assert.AreEqual(1, GetQueueSize(queueName));
            listener.Start();
            Thread.Sleep(1000);
            Assert.AreEqual(1, GetQueueSize(queueName + "-dead"));
            Assert.AreEqual(0, GetQueueSize(queueName));
            Assert.AreEqual(1, listener.GetCountRetry());

            var received = Messaging.Dequeue<EnterpriseIntegrationMessage>(queueName + "-dead", config);

            Assert.AreEqual(0, GetQueueSize(queueName + "-dead"));
            Assert.AreEqual(message.MessageID, received.MessageID);
            Assert.AreEqual(0, GetQueueSize(queueName));
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