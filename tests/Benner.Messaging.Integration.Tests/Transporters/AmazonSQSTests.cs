﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Tests.Transporters
{
    [TestClass]
    public class AmazonSqsTests : TransporterBaseTests
    {
        [TestMethod]
        public void AmazonSQS_deve_enviar_e_receber_mensagens_com_configuracao_em_memoria()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-amazonsqs-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new MessagingConfigBuilder("AmazonSQS", BrokerType.AmazonSQS, new Dictionary<string, string>()
            {
                {"InvisibilityTime", "15"}
            })
            .Create();

            Messaging.Enqueue(queueName, message, config);
            var received = Messaging.Dequeue(queueName, config);

            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void AmazonSQS_deve_consumir_mensagem_lancando_exception_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-amazonsqs-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AmazonSQS));

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
        public void AmazonSQS_deve_consumir_mensagem_retornando_false_e_verificar_que_ela_continua_na_fila()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-amazonsqs-{guid}";
            string message = $"Mensagem que deve retornar {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AmazonSQS));

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
        public void AmazonSQS_deve_enviar_e_receber_mensagem()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-amazonsqs-{guid}";
            string message = $"Mensagem teste para AmazonSQS with guid {guid}";
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AmazonSQS));
            Messaging.Enqueue(queueName, message, config);
            string received = Messaging.Dequeue(queueName, config);
            Assert.AreEqual(message, received);
        }

        [TestMethod]
        public void AmazonSQS_deve_enviar_e_receber_objeto_serializado()
        {
            var config = new FileMessagingConfig(LoadFileConfig(Broker.AmazonSQS));
            string queueName = $"fila-teste-amazonsqs-{Guid.NewGuid()}";
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
        public void AmazonSQS_deve_receber_nulo_ao_ouvir_mensagem_inexistente()
        {
            var single = Messaging.Dequeue($"fila-teste-amazonsqs-{Guid.NewGuid()}", new FileMessagingConfig(LoadFileConfig(Broker.AmazonSQS)));
            Assert.IsNull(single);
        }

        [TestMethod]
        public void AmazonSQS_deve_lancar_erro_ao_deserializar_messagem_de_tipos_diferentes()
        {
            var guid = Guid.NewGuid().ToString();
            string queueName = $"fila-teste-amazonsqs-{guid}";
            var config = new MessagingConfigBuilder("AmazonSQS", BrokerType.AmazonSQS, new Dictionary<string, string>()
                {
                    {"InvisibilityTime", "15"}
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
