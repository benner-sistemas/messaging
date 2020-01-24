using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Benner.Messaging.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class ActiveMqTests : RetryTestsBase
    {
        private readonly ConnectionFactory _factory = new ConnectionFactory("tcp://bnu-vtec001:61616");

        public ActiveMqTests() : base(new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {   {"UserName", "admin"},
                {"Password", "admin"},
                {"Hostname", "bnu-vtec001" },
                {"Port", "61616" }
            }).Create())
        { }


       [TestInitialize]
        public void Initializer()
        {
            Log.ConfigureLog();
        }

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas_com_activeMQ()
        {
            base.EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas();
        }

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas_com_activeMQ()
        {
            base.EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas();
        }

        protected override void PurgeQueue(string queueName)
        {
            using (Connection connection = _factory.CreateConnection() as Connection)
            using (ISession session = connection.CreateSession())
            {
                IQueue queue = session.GetQueue(queueName);
                try
                {
                    connection.DeleteDestination(queue);
                }
                catch
                { /*ignore errros*/ }
            }
        }

        protected override void CreateQueue(string queueName)
        {
            using (Connection conn = _factory.CreateConnection() as Connection)
            {
                using (ISession session = conn.CreateSession())
                {
                    session.GetQueue(queueName);
                }
            }
        }

        protected override int GetQueueSize(string queueName)
        {
            int count = 0;
            using (Connection conn = _factory.CreateConnection() as Connection)
            {
                conn.Start();
                using (ISession session = conn.CreateSession())
                {
                    IQueue queue = session.GetQueue(queueName);
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