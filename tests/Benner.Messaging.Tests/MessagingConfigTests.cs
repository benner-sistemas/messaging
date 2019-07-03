using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Benner.Messaging.Tests
{
    [TestClass]
    public class MessagingConfigTests
    {
        private readonly string _testeConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teste.config");

        [TestInitialize, TestCleanup]
        public void Init()
        {
            if (File.Exists(_testeConfigPath))
                File.Delete(_testeConfigPath);
        }

        [TestMethod]
        public void Client_deve_suportar_injecao_de_configuracao_sem_arquivo()
        {
            var mmc = new MemoryMessageConfig(defaultBroker: "MockMQ", defaultBrokerConfigType: typeof(MockMQConfig))
                            .WithQueue("teste1", "MockMQ")
                            .WithQueue("teste2", "RabbitMQ");

            string queueName = "teste1";
            string sentMsg = "Teste de configuração em memória";


            Messaging.Enqueue(queueName, sentMsg, mmc);

            string received = "";
            using (var client = new Messaging(mmc))
            {
                client.StartListening(queueName, (args) =>
                 {
                     received = args.AsString;
                     return true;
                 });
            }

            Assert.AreEqual(sentMsg, received);
        }

        [TestMethod]
        public void Configuracao_deve_devolver_broker_padrao_para_fila_nao_configurada()
        {
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?><configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection>
                    <queues>
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

            File.AppendAllText(_testeConfigPath, fileContent);

            string queueName = "broker-padrao-para-fila-nao-configurada";
            var sentMsg = "Teste automatizado da fila broker_padrao_para_fila_nao_configurada";

            var config = new FileMessagingConfig(_testeConfigPath);
            var configType = config.GetConfigForQueue(queueName);
            Assert.IsNotNull(configType);
            Assert.AreEqual(typeof(MockMQConfig).FullName, configType.GetType().FullName);

            Messaging.Enqueue(queueName, sentMsg, config);
            string received = Messaging.Dequeue(queueName, config);
            Assert.AreEqual(sentMsg, received);
        }

        [TestMethod]
        public void Configuracao_deve_devolver_broker_especifico_para_fila_informada()
        {
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?><configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection>
                    <queues>
                        <queue name='broker-especifico-para-fila-informada' broker='MockMQ' />
                    </queues>
                    <brokerList default='RabbitMQ'> 
                        <broker name='RabbitMQ' type='Benner.Messaging.RabbitMQConfig, Benner.Messaging'>
                            <add key='chave' value='valor' />
                        </broker>
                        <broker name='MockMQ' type='Benner.Messaging.Tests.MockMQConfig, Benner.Messaging.Tests'>
                            <add key='chave' value='valor' />
                        </broker>
                    </brokerList>
                    </MessagingConfigSection></configuration>";

            File.AppendAllText(_testeConfigPath, fileContent);

            string queueName = "broker-especifico-para-fila-informada";
            var sentMsg = "Teste automatizado da fila broker_especifico_para_fila_informada";

            var config = new FileMessagingConfig(_testeConfigPath);
            Messaging.Enqueue(queueName, sentMsg, config);
            string received = Messaging.Dequeue(queueName, config);
            Assert.AreEqual(sentMsg, received);
        }

        [TestMethod]
        public void Configuracao_deve_emitir_exception_caso_nao_tenha_nenhum_broker_configurado()
        {
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?>
                    <configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection>
                        <queues></queues>
                        <brokerList default='RabbitMQ'>
                        </brokerList>
                    </MessagingConfigSection></configuration>";

            File.AppendAllText(_testeConfigPath, fileContent);

            Assert.ThrowsException<XmlException>(() => new FileMessagingConfig(_testeConfigPath));
        }

        [TestMethod]
        public void Configuracao_deve_emitir_exception_ao_tentar_recuperar_broker_inexistente_de_fila_configurada()
        {
            // arquivo
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?><configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection>
                    <queues>
                        <queue name='teste' broker='naoexiste' />
                    </queues>
                    <brokerList default='RabbitMQ'> 
                        <broker name='RabbitMQ' type='Benner.Messaging.RabbitMQConfig, Benner.Messaging'>
                            <add key='chave' value='valor' />
                        </broker>
                    </brokerList>
                    </MessagingConfigSection></configuration>";

            File.AppendAllText(_testeConfigPath, fileContent);
            Assert.ThrowsException<ArgumentException>(() => new FileMessagingConfig(_testeConfigPath).GetConfigForQueue("teste"));

            // memória
            var config = MessagingConfigFactory
                .NewMessagingConfigFactory()
                .WithRabbitMQBroker("",0,"","")
                .WithMappedQueue("teste", "naoexiste")
                .Create();
            Assert.ThrowsException<ArgumentException>(() => config.GetConfigForQueue("teste"));
        }

        [TestMethod]
        public void Configuracao_de_arquivo_deve_emitir_exception_para_nomes_invalidos_de_fila_no_load()
        {
            var fileContentPart1 = @"<?xml version='1.0' encoding='utf-8' ?><configuration><configSections><section name='MessagingConfigSection' 
                type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' /></configSections><MessagingConfigSection><queues>";

            var devemReprovar = new string[]
            {
                "<queue name='aa' broker='naoexiste' />", "<queue name='a' broker='naoexiste' />", "<queue name='a_a' broker='naoexiste' />",
                "<queue name='MA' broker='naoexiste' />", "<queue name='e#%' broker='naoexiste' />",
                "<queue name='aaaaaaaaaaaaaaaaaaaa-aaaaaaaaaaaaaaaaaaaa-aaaaaaaaaaaaaaaaaaaa-' broker='naoexiste' />",
                "<queue name='aaaa--a' broker='naoexiste' />", "<queue name='-aaaaaaaaaa' broker='naoexiste' />",
                "<queue name='aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' broker='naoexiste' />",
                "<queue name='maiUSCULA' broker='naoexiste' />"
            };

            var fileContentPart3 = @"</queues><brokerList default='MockMQ'><broker name='MockMQ' type='Benner.Messaging.Tests.MockMQConfig, 
                Benner.Messaging.Tests'><add key='chave' value='valor' /></broker></brokerList></MessagingConfigSection></configuration>";

            foreach (string queueTag in devemReprovar)
            {
                File.AppendAllText(_testeConfigPath, fileContentPart1);
                File.AppendAllText(_testeConfigPath, queueTag);
                File.AppendAllText(_testeConfigPath, fileContentPart3);
                Assert.ThrowsException<ArgumentException>(() => new FileMessagingConfig(_testeConfigPath), $"Tag que falhou: {queueTag}");
                File.Delete(_testeConfigPath);
            }
        }

        [TestMethod]
        public void Configuracao_de_memoria_deve_emitir_exception_para_nomes_invalidos_quando_adicionados()
        {
            var devemReprovar = new string[]
            {
                "a",
                "aa",
                "MA",
                "a_a",
                "e#%",
                "aaaa--a",
                "maiUSCULA",
                "-aaaaaaaaaa",
                "aaaaaaaaaaaaaaaaaaaa-aaaaaaaaaaaaaaaaaaaa-aaaaaaaaaaaaaaaaaaaa-",
                "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
            };

            foreach (string queueName in devemReprovar)
            {
                
                Assert.ThrowsException<ArgumentException>(() =>
                    MessagingConfigFactory
                        .NewMessagingConfigFactory()
                        .WithMappedQueue(queueName, "teste")
                        .Create(), 
                    $"Tag que falhou: {queueName}");
            }
        }

        [TestMethod]
        public void Configuracao_nao_deve_aceitar_nome_de_broker_default_cuja_configuracao_nao_existe()
        {
            var fileContent = @"<?xml version='1.0' encoding='utf-8' ?><configuration><configSections>
                    <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections><MessagingConfigSection><queues></queues>
                    <brokerList default='qualquernome'> 
                        <broker name='MockMQ' type='Benner.Messaging.Tests.MockMQConfig, Benner.Messaging.Tests'>
                        </broker>
                    </brokerList>
                    </MessagingConfigSection></configuration>";

            File.AppendAllText(_testeConfigPath, fileContent);

            Assert.ThrowsException<XmlException>(() => new FileMessagingConfig(_testeConfigPath));
        }
    }
}