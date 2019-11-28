using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Benner.Messaging;
using System.Collections.Generic;
using Benner.Messaging.Tests;

namespace Benner.Messaging.Retry.Tests
{
    [TestClass]
    public class ActiveMqTests : TransporterBaseTests
    {
        [TestMethod]
        public void Testa_retentativa()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-activemq-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new MessagingConfigBuilder("ActiveMQ", BrokerType.ActiveMQ, new Dictionary<string, string>()
            {
                {"Hostname", "bnu-vtec001" }
            }).Create();

            Messaging.Enqueue(queueName, message, config);
            var received = Messaging.Dequeue(queueName, config);

            Assert.AreEqual(message, received);
        }
    }
}
