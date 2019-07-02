using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Benner.Messaging.Tests
{
    [TestClass]
    public class ClientTests
    {
        private readonly string folder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        [TestInitialize, TestCleanup]
        public void Init()
        {
            QueueMock.ClearAllQueues();
            foreach (var item in Directory.GetFiles(folder, "*.config"))
                if (!item.EndsWith("messaging.config"))
                    File.Delete(item);
        }

        [TestMethod]
        public void Client_deve_permitir_receber_qualquer_IMessagingConfig()
        {
            string mensagem = "Mensagem para teste de fila";
            // por memoria
            var memoryConfig = new MemoryMessageConfig(defaultBroker: "MockMQ", defaultBrokerConfigType: typeof(MockMQConfig)).WithQueue("teste", "MockMQ");

            string receivedByMemory = "";
            new Client(memoryConfig).EnqueueMessage("teste", mensagem);
            new Client(memoryConfig).StartListening("teste", (args) =>
            {
                receivedByMemory = args.AsString;
                return true;
            });

            Assert.AreEqual(mensagem, receivedByMemory);

            // por arquivo
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?><configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection><queues><queue name='teste-arquivo' broker='MockMQ' /></queues><brokerList default='MockMQ'>
                    <broker name='RabbitMQ' type='Benner.Messaging.RabbitMQConfig, Benner.Messaging'><add key='chave' value='valor' /></broker>
                    <broker name='MockMQ' type='Benner.Messaging.Tests.MockMQConfig, Benner.Messaging.Tests'><add key='chave' value='valor' /></broker>
                    </brokerList></MessagingConfigSection></configuration>";

            var path = Path.Combine(folder, "teste.config");
            File.AppendAllText(path, fileContent);

            var fileConfig = new FileMessagingConfig(path);

            string receivedByFile = "";

            new Client(fileConfig).EnqueueMessage("teste-arquivo", mensagem);
            new Client(fileConfig).StartListening("teste-arquivo", (args) =>
            {
                receivedByFile = args.AsString;
                return true;
            });

            Assert.AreEqual(mensagem, receivedByFile);
        }

        [TestMethod]
        public void Client_deve_assumir_configuracao_por_arquivo_caso_nenhum_IMessageConfig_seja_informado()
        {
            //Garantir que existe um arquivo de configuração antes
            string fullPath = Path.Combine(folder, "messaging.config");
            if (!File.Exists(fullPath))
                Assert.Fail("Arquivo não encontrado.");

            var clientSemException = new Client();
            clientSemException.Dispose();

            //renomear o arquivo
            var file = new FileInfo(fullPath);
            file.MoveTo(Path.Combine(folder, "renomeado.txt"));

            //Instancia deve não encontrar mais o arquivo
            Assert.ThrowsException<FileNotFoundException>(() => new Client());

            //Des-renomear
            file.MoveTo(fullPath);
        }

        [TestMethod]
        public void Client_deve_emitir_excecao_caso_arquivo_padrao_nao_seja_encontrado()
        {
            string fileName = "messaging.config";
            string fullPath = Path.Combine(folder, fileName);

            if (!File.Exists(fullPath))
                Assert.Fail("Arquivo não encontrado.");

            var file = new FileInfo(fullPath);
            file.MoveTo(Path.Combine(folder, "renomeado.txt"));

            Assert.ThrowsException<FileNotFoundException>(() => new FileMessagingConfig());

            file.MoveTo(fullPath);
        }

        [TestMethod]
        public void Client_deve_ser_case_insensitive_ao_lidar_com_filas()
        {
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?>
                    <configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection>
                        <queues>
                            <queue name='teste-case-insensitive' broker='MockMQ' />
                        </queues>
                        <brokerList default='MockMQ'>
                            <broker name='RabbitMQ' type='Benner.Messaging.RabbitMQConfig, Benner.Messaging'>
                                <add key='chave' value='valor' />
                            </broker>
                            <broker name='MockMQ' type='Benner.Messaging.Tests.MockMQConfig, Benner.Messaging.Tests'>
                                <add key='chave' value='valor' />
                            </broker>
                        </brokerList>
                    </MessagingConfigSection></configuration>";

            var path = Path.Combine(folder, "teste.config");
            File.AppendAllText(path, fileContent);

            var sentMsg = "Teste de fila case insensitive";

            new Client(new FileMessagingConfig(path)).EnqueueMessage("testE-caSE-inSEnsitive", sentMsg);

            string received = "";
            new Client(new FileMessagingConfig(path)).StartListening("testE-caSE-inSeNsITIve", (args) =>
            {
                received = args.AsString;
                return true;
            });

            Assert.AreEqual(sentMsg, received);
        }

        [TestMethod]
        public void Client_deve_ser_case_insensitive_ao_lidar_com_broker_name()
        {
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?>
                    <configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection>
                        <queues>
                            <queue name='teste-case-insensitive-brokername' broker='mockmq' />
                        </queues>
                        <brokerList default='MockMQ'>
                            <broker name='RabbitMQ' type='Benner.Messaging.RabbitMQConfig, Benner.Messaging'>
                                <add key='chave' value='valor' />
                            </broker>
                            <broker name='MockmQ' type='Benner.Messaging.Tests.MockMQConfig, Benner.Messaging.Tests'>
                                <add key='chave' value='valor' />
                            </broker>
                        </brokerList>
                    </MessagingConfigSection></configuration>";

            var path = Path.Combine(folder, "teste.config");
            File.AppendAllText(path, fileContent);

            string queueName = "teste-CASE-insensitive-BROKERname";
            var sentMsg = "Teste de broker case insensitive";

            new Client(new FileMessagingConfig(path)).EnqueueMessage(queueName, sentMsg);

            string received = "";
            new Client(new FileMessagingConfig(path)).StartListening(queueName, (args) =>
            {
                received = args.AsString;
                return true;
            });

            Assert.AreEqual(sentMsg, received);
        }


        [TestMethod]
        public void EnqueueMessage_and_StartListening_must_deal_with_generic_objects()
        {
            QueueMock.ClearAllQueues();

            var message = new Invoice
            {
                AccountReference = Guid.NewGuid().ToString(),
                CurrencyUsed = false,
                CustomerOrderNumber = int.MaxValue,
                ForeignRate = 352.65m,
                InvoiceDate = DateTime.Now,
                InvoiceNumber = int.MaxValue,
                ItemsNet = int.MinValue,
                ItemsTax = 65645.98m,
                ItemsTotal = 321564m,
                OrderNumber = 321123,
            };


            var mockMQConfig = new MemoryMessageConfig(defaultBroker: "MockMQ", defaultBrokerConfigType: typeof(MockMQConfig));
            using (var client = new Client(mockMQConfig))
            {
                client.EnqueueMessage("invoices-a", message);
            }


            using (var client = new Client(mockMQConfig))
            {
                client.StartListening("invoices-a", (args) =>
                {
                    var typedMessage = args.GetMessage<Invoice>();
                    Assert.AreEqual(message.AccountReference, typedMessage.AccountReference);
                    Assert.AreEqual(message.CurrencyUsed, typedMessage.CurrencyUsed);
                    Assert.AreEqual(message.CustomerOrderNumber, typedMessage.CustomerOrderNumber);
                    Assert.AreEqual(message.ForeignRate, typedMessage.ForeignRate);
                    Assert.AreEqual(message.InvoiceDate, typedMessage.InvoiceDate);
                    Assert.AreEqual(message.InvoiceNumber, typedMessage.InvoiceNumber);
                    Assert.AreEqual(message.ItemsNet, typedMessage.ItemsNet);
                    Assert.AreEqual(message.ItemsTax, typedMessage.ItemsTax);
                    Assert.AreEqual(message.ItemsTotal, typedMessage.ItemsTotal);
                    Assert.AreEqual(message.OrderNumber, typedMessage.OrderNumber);

                    return true;
                });
                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void EnqueueSingleMessage_and_StartListening_must_deal_with_generic_objects()
        {
            QueueMock.ClearAllQueues();

            var message = new Invoice
            {
                AccountReference = Guid.NewGuid().ToString(),
                CurrencyUsed = false,
                CustomerOrderNumber = int.MaxValue,
                ForeignRate = 352.65m,
                InvoiceDate = DateTime.Now,
                InvoiceNumber = int.MaxValue,
                ItemsNet = int.MinValue,
                ItemsTax = 65645.98m,
                ItemsTotal = 321564m,
                OrderNumber = 321123,
            };

            var mockMQConfig = new MemoryMessageConfig(defaultBroker: "MockMQ", defaultBrokerConfigType: typeof(MockMQConfig));

            Client.EnqueueSingleMessage("invoices-b", message, mockMQConfig);

            var typedMessage = Client.DequeueSingleMessage<Invoice>("invoices-b", mockMQConfig);

            Assert.AreEqual(message.AccountReference, typedMessage.AccountReference);
            Assert.AreEqual(message.CurrencyUsed, typedMessage.CurrencyUsed);
            Assert.AreEqual(message.CustomerOrderNumber, typedMessage.CustomerOrderNumber);
            Assert.AreEqual(message.ForeignRate, typedMessage.ForeignRate);
            Assert.AreEqual(message.InvoiceDate, typedMessage.InvoiceDate);
            Assert.AreEqual(message.InvoiceNumber, typedMessage.InvoiceNumber);
            Assert.AreEqual(message.ItemsNet, typedMessage.ItemsNet);
            Assert.AreEqual(message.ItemsTax, typedMessage.ItemsTax);
            Assert.AreEqual(message.ItemsTotal, typedMessage.ItemsTotal);
            Assert.AreEqual(message.OrderNumber, typedMessage.OrderNumber);
        }
    }
}