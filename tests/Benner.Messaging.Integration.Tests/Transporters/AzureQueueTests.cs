using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Tests.Transporters
{
    [TestClass]
    public class AzureQueueTests : TransporterBaseTests
    {
        [TestMethod]
        public void AzureQueue_deve_enviar_e_receber_mensagens_com_configuracao_em_memoria()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-azure-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new MessagingConfigBuilder("AzureQueue", BrokerType.AzureQueue, new Dictionary<string, string>()
                {
                    {"InvisibilityTime", "15"},
                    {"ConnectionString", AzureConnectionString},
                })
                .Create();

            Messaging.Enqueue(queueName, message, config);
            var received = Messaging.Dequeue(queueName, config);

            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void AzureQueue_deve_consumir_mensagem_lancando_exception_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-azure-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AzureQueue));

            Messaging.Enqueue(queueName, message, config);

            using (var receiver = new Messaging(config))
            {
                receiver.StartListening(queueName, (args) =>
                {
                    throw new Exception("Vai para fila de error.");
                });
                System.Threading.Thread.Sleep(100);
            }

            var received = Messaging.Dequeue(queueName, config);
            Assert.IsNull(received);

            var errorMessage = Messaging.Dequeue($"{queueName}-error", config);
            Assert.AreEqual($"Vai para fila de error.\r\n{message}", errorMessage);
        }

        [TestMethod]
        public void AzureQueue_deve_consumir_mensagem_retornando_false_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-azure-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AzureQueue));

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
        public void AzureQueue_deve_enviar_e_receber_mensagem()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-azurequeue-{guid}";
            string message = $"Mensagem teste para Azure Queue with guid {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AzureQueue));
            Messaging.Enqueue(queueName, message, config);
            string received = Messaging.Dequeue(queueName, config);
            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void AzureQueue_deve_enviar_e_receber_objeto_serializado()
        {
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AzureQueue));
            string queueName = $"fila-teste-azurequeue-{Guid.NewGuid()}";
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
        public void AzureQueue_deve_receber_nulo_ao_ouvir_mensagem_inexistente()
        {
            var single = Messaging.Dequeue($"fila-teste-azurequeue-{Guid.NewGuid()}", new FileMessagingConfig(LoadFileConfig(Broker.AzureQueue)));
            Assert.IsNull(single);
        }
    }
}