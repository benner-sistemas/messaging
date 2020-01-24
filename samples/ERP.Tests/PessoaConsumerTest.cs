using Benner.ERP.Models;
using Benner.Listener;
using Benner.Messaging.Configuration;
using Benner.Messaging.Logger;
using ERP.Consumer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ERP.Tests
{
    [TestClass]
    public class PessoaConsumerTest
    {
        [TestInitialize]
        public void Initializer()
        {
            Log.ConfigureLog();
        }

        [TestMethod]
        public void ConfiguracoesDePessoaConsumerDeveSerPreenchidaPorPadrao()
        {
            IEnterpriseIntegrationConsumer consumer = new PessoaConsumer();
            var settings = consumer.Settings;
            Assert.AreEqual("fila-pessoa-consumer", settings.QueueName);
            Assert.AreEqual(1000, settings.RetryIntervalInMilliseconds);
            Assert.AreEqual(3, settings.RetryLimit);
        }

        [TestMethod]
        public void ConsumerDeveLancarExceptionAoReceberParametrosInvalidos()
        {
            IEnterpriseIntegrationConsumer consumer = new PessoaConsumer();
            var request = new PessoaRequest();

            // a ordem importa
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(JsonParser.Serialize(request)));
            request.CPF = "01234567890";
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(JsonParser.Serialize(request)));
            request.Nascimento = DateTime.Now;
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(JsonParser.Serialize(request)));
            request.Nome = "Ciclano Butano";
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(JsonParser.Serialize(request)));
            request.RequestID = Guid.NewGuid();
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(JsonParser.Serialize(request)));
            request.Endereco = new Endereco();

            consumer.OnMessage(JsonParser.Serialize(request));
        }

        [TestMethod]
        public void ConsumerDeveLancarExceptionAoReceberRequestNula()
        {
            IEnterpriseIntegrationConsumer consumer = new PessoaConsumer();
            object request = null;

            Assert.ThrowsException<ArgumentNullException>(() => consumer.OnMessage(JsonParser.Serialize(request)));
            Assert.ThrowsException<ArgumentNullException>(() => consumer.OnInvalidMessage(JsonParser.Serialize(request), new InvalidMessageException()));
            Assert.ThrowsException<ArgumentNullException>(() => consumer.OnDeadMessage(JsonParser.Serialize(request), new Exception()));
        }
    }
}