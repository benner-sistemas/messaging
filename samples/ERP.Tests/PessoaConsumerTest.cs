using Benner.ERP.Models;
using Benner.Listener;
using ERP.Consumer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ERP.Tests
{
    [TestClass]
    public class PessoaConsumerTest
    {
        [TestMethod]
        public void ConfiguracoesDePessoaConsumerDeveSerPreenchidaPorPadrao()
        {
            IEnterpriseIntegrationConsumer consumer = new PessoaConsumer();
            var settings = consumer.Settings;
            Assert.AreEqual("nome-da-fila", settings.QueueName);
            Assert.AreEqual(1000, settings.RetryIntervalInMilliseconds);
            Assert.AreEqual(3, settings.RetryLimit);
        }

        [TestMethod]
        public void ConsumerDeveLancarExceptionAoReceberParametrosInvalidos()
        {
            IEnterpriseIntegrationConsumer consumer = new PessoaConsumer();
            var request = new PessoaRequest();

            // a ordem importa
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(request));
            request.CPF = "01234567890";
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(request));
            request.Nascimento = DateTime.Now;
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(request));
            request.Nome = "Ciclano Butano";
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(request));
            request.RequestID = Guid.NewGuid();
            Assert.ThrowsException<InvalidMessageException>(() => consumer.OnMessage(request));
            request.Endereco = new Endereco();

            consumer.OnMessage(request);
        }

        [TestMethod]
        public void ConsumerDeveLancarExceptionAoReceberRequestNula()
        {
            IEnterpriseIntegrationConsumer consumer = new PessoaConsumer();
            object request = null;

            Assert.ThrowsException<ArgumentNullException>(() => consumer.OnMessage(request));
            Assert.ThrowsException<ArgumentNullException>(() => consumer.OnInvalidMessage(request));
            Assert.ThrowsException<ArgumentNullException>(() => consumer.OnDeadMessage(request));
        }
    }
}