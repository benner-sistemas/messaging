using Benner.Messaging.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Tests.Transporters
{
    [TestClass]
    public class RabbitMqTests : TransporterBaseTests
    {
        [TestInitialize]
        public void Initialize()
        {
            Log.ConfigureLog();
        }

        [TestMethod]
        public void RabbitMQ_deve_enviar_e_receber_mensagens_com_configuracao_em_memoria()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new MessagingConfigBuilder("RabbitMQ", BrokerType.RabbitMQ, new Dictionary<string, string>()
                {
                    {"UserName", "guest"},
                    {"Password", "guest"},
                    {"HostName", RabbitMQServerName}
                })
                .Create();

            Messaging.Enqueue(queueName, message, config);
            var received = Messaging.Dequeue(queueName, config);

            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void RabbitMQ_deve_consumir_mensagem_lancando_exception_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            var queueName = new QueueName($"fila-teste-rabbitmq-{guid}");
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ));

            Messaging.Enqueue(queueName.Default, message, config);

            using (var receiver = new Messaging(config))
            {
                receiver.StartListening(queueName.Default, (args) =>
                {
                    throw new Exception("Vai para fila de error.");
                });
                System.Threading.Thread.Sleep(100);
            }

            var received = Messaging.Dequeue(queueName.Default, config);
            Assert.IsNull(received);

            var errorMessage = Messaging.Dequeue(queueName.Dead, config);
            Assert.AreEqual($"Vai para fila de error.\r\n{message}", errorMessage);
        }

        [TestMethod]
        public void RabbitMQ_deve_consumir_mensagem_retornando_false_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ));

            Messaging.Enqueue(queueName, message, config);

            string received = "";
            bool isFirst = true;
            int attempts = 0;
            using (var receiver = new Messaging(config))
            {
                bool shouldRun = true;
                receiver.StartListening(queueName, (args) =>
                {
                    if (shouldRun)
                    {
                        attempts++;
                        if (isFirst)
                        {
                            isFirst = false;
                            return false;
                        }
                        received = args.AsString;
                        shouldRun = false;
                        return true;
                    }
                    return true;
                });
                while (shouldRun)
                    System.Threading.Thread.Sleep(20);
            }
            Assert.AreEqual(message, received);
            Assert.AreEqual(2, attempts);
        }

        [TestMethod]
        public void RabbitMQ_deve_enviar_e_receber_mensagem()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            string message = $"Mensagem teste para RabbitMQ with guid {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ));
            using (var sender = new Messaging(config))
                sender.EnqueueMessage(queueName, message);

            string received = "";
            using (var receiver = new Messaging(config))
            {
                System.Threading.AutoResetEvent waiter = new System.Threading.AutoResetEvent(false);
                receiver.StartListening(queueName, (args) =>
                {
                    received = args.AsString;
                    waiter.Set();
                    return true;
                });
                waiter.WaitOne();
            }
            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void RabbitMQ_deve_enviar_e_receber_objeto_serializado()
        {
            var config = new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ));
            string queueName = $"fila-teste-rabbitmq-{Guid.NewGuid()}";

            Messaging.Enqueue(queueName, _invoiceMessage, config);
            var typedMessage = Messaging.Dequeue<Invoice>(queueName, config);

            Assert.AreEqual(_invoiceMessage.AccountReference, typedMessage.AccountReference);
            Assert.AreEqual(_invoiceMessage.CurrencyUsed, typedMessage.CurrencyUsed);
            Assert.AreEqual(_invoiceMessage.CustomerOrderNumber, typedMessage.CustomerOrderNumber);
            Assert.AreEqual(_invoiceMessage.ForeignRate, typedMessage.ForeignRate);
            Assert.AreEqual(_invoiceMessage.InvoiceDate, typedMessage.InvoiceDate);
            Assert.AreEqual(_invoiceMessage.InvoiceNumber, typedMessage.InvoiceNumber);
            Assert.AreEqual(_invoiceMessage.ItemsNet, typedMessage.ItemsNet);
            Assert.AreEqual(_invoiceMessage.ItemsTax, typedMessage.ItemsTax);
            Assert.AreEqual(_invoiceMessage.ItemsTotal, typedMessage.ItemsTotal);
            Assert.AreEqual(_invoiceMessage.OrderNumber, typedMessage.OrderNumber);
        }

        [TestMethod]
        public void RabbitMQ_deve_receber_nulo_ao_ouvir_mensagem_inexistente()
        {
            var single = Messaging.Dequeue($"fila-teste-rabbitmq-{Guid.NewGuid()}", new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ)));
            Assert.IsNull(single);
        }

        [TestMethod]
        public void RabbitMQ_deve_lancar_erro_ao_deserializar_messagem_de_tipos_diferentes()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            var config = new MessagingConfigBuilder("RabbitMQ", BrokerType.RabbitMQ, new Dictionary<string, string>()
                {
                    { "HostName", RabbitMQServerName }
                }).Create();

            // garantir que a fila tem 0 mensagens
            var vazia = Messaging.Dequeue(queueName, config);
            Assert.IsNull(vazia);

            // enviar um objeto Invoice
            Messaging.Enqueue(queueName, _invoiceMessage, config);

            // receber objeto convertendo pra AnotherClass
            Assert.ThrowsException<InvalidCastException>(() => Messaging.Dequeue<AnotherClass>(queueName, config));

            // garantir que ainda está na fila recebendo uma mensagem
            var recebida = Messaging.Dequeue(queueName, config);
            Assert.IsNotNull(recebida);
        }
    }
}