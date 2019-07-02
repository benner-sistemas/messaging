using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Tests.Transporters
{
    [TestClass]
    public class RabbitMqTests : TransporterBaseTests
    {
        [TestMethod]
        public void RabbitMQ_deve_enviar_e_receber_mensagens_com_configuracao_em_memoria()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new MemoryMessagingConfigBuilder("RabbitMQ", Messaging.Broker.Rabbit, new Dictionary<string, string>()
            {
                {"UserName", "guest"},
                {"Password", "guest"},
                {"HostName", ServerName}
            })
            .Create();

            Client.EnqueueSingleMessage(queueName, message, config);
            var received = Client.DequeueSingleMessage(queueName, config);

            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void RabbitMQ_deve_consumir_mensagem_lancando_exception_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ));

            Client.EnqueueSingleMessage(queueName, message, config);

            using (var receiver = new Client(config))
            {
                receiver.StartListening(queueName, (args) =>
                {
                    throw new Exception("Vai para fila de error.");
                });
                System.Threading.Thread.Sleep(100);
            }

            var received = Client.DequeueSingleMessage(queueName, config);
            Assert.IsNull(received);

            var errorMessage = Client.DequeueSingleMessage($"{queueName}-error", config);
            Assert.AreEqual($"Vai para fila de error.\r\n{message}", errorMessage);
        }

        [TestMethod]
        public void RabbitMQ_deve_consumir_mensagem_retornando_false_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-rabbitmq-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ));

            Client.EnqueueSingleMessage(queueName, message, config);

            string received = "";
            bool isFirst = true;
            int attempts = 0;
            using (var receiver = new Client(config))
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
            using (Client sender = new Client(config))
                sender.EnqueueMessage(queueName, message);

            string received = "";
            using (var receiver = new Client(config))
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

            Client.EnqueueSingleMessage(queueName, _invoiceMessage, config);
            var typedMessage = Client.DequeueSingleMessage<Invoice>(queueName, config);

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
            var single = Client.DequeueSingleMessage($"fila-teste-rabbitmq-{Guid.NewGuid()}", new FileMessagingConfig(LoadFileConfig(Broker.RabbitMQ)));
            Assert.IsNull(single);
        }
    }
}