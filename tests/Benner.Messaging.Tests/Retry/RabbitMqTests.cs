using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class RabbitMQTests : RetryTestsBase
    {
        private static ConnectionFactory _factory = new ConnectionFactory
        {
            HostName = "bnu-vtec012",
            UserName = "guest",
            Port = 5672
        };
        public RabbitMQTests() : base(new MessagingConfigBuilder("RabbitMQ", BrokerType.RabbitMQ, new Dictionary<string, string>()
                {
                    {"UserName", "guest"},
                    {"Password", "guest"},
                    {"HostName", "bnu-vtec012"}
                })
                .Create())
        { }

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas_com_rabbitMQ()
        {
            base.EnterpriseIntegrationListener_deve_disparar_fluxo_de_retentativas();
        }

        [TestMethod]
        public void EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas_com_rabbitMQ()
        {
            base.EnterpriseIntegrationListener_deve_direcionar_para_fila_de_imagens_invalidas();
        }

        protected override void PurgeQueue(string queueName)
        {
            using (var conn = _factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueuePurge(queueName);
                }
            }
        }

        protected override void CreateQueue(string queueName)
        {
            using (var conn = _factory.CreateConnection())
            {
                using (var channel = conn.CreateModel())
                {
                    channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                }
            }
        }

        protected override int GetQueueSize(string queueName)
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
