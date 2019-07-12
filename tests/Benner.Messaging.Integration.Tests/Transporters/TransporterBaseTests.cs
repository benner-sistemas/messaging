using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Benner.Messaging.Tests
{
    [TestClass]
    public class TransporterBaseTests
    {
        internal static string ServerName = "servername";
        internal static string AzureConnectionString = "DefaultEndpointsProtocol=https;AccountName=[account-name];AccountKey=[account-key];EndpointSuffix=core.windows.net";

        protected enum Broker { ActiveMQ, AmazonSQS, AzureQueue, RabbitMQ }
        protected Invoice _invoiceMessage = null;

        [TestInitialize]
        public void Init()
        {
            _invoiceMessage = new Invoice
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
                OrderNumber = 321123
            };
            DeleteFileConfig();
        }

        [TestCleanup]
        public void CleanUp() => DeleteFileConfig();

        protected void DeleteFileConfig()
        {
            string testConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teste.config");
            if (File.Exists(testConfigPath))
                File.Delete(testConfigPath);
        }

        protected string LoadFileConfig(Broker defaultBroker)
        {
            var content =
                $@"<?xml version='1.0' encoding='utf-8' ?>
                <configuration>
                    <configSections>
                        <section name='MessagingConfigSection' type='Benner.Messaging.MessagingFileConfigSection, Benner.Messaging' />
                    </configSections>
                    <MessagingConfigSection>
                        <queues>
                          </queues>
                        <brokerList default='{defaultBroker.ToString()}'> 
                            <broker name='ActiveMQ' type='Benner.Messaging.ActiveMQConfig, Benner.Messaging'>
                              <add key='Hostname' value='{ServerName}' />
                            </broker>
                            <broker name='AmazonSQS' type='Benner.Messaging.AmazonSQSConfig, Benner.Messaging'>
                              <add key='InvisibilityTime' value='15' />
                            </broker>
                            <broker name='AzureQueue' type='Benner.Messaging.AzureQueueConfig, Benner.Messaging'>
                              <add key='ConnectionString' value='{AzureConnectionString}' />
                              <add key='InvisibilityTime' value='15' />
                            </broker>
                            <broker name='RabbitMQ' type='Benner.Messaging.RabbitMQConfig, Benner.Messaging'>
                              <add key='UserName' value='guest' />
                              <add key='Password' value='guest' />
                              <add key='HostName' value='{ServerName}' />
                            </broker>
                        </brokerList>
                    </MessagingConfigSection>
                </configuration>";

            string testConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teste.config");
            File.AppendAllText(testConfigPath, content);
            return testConfigPath;
        }
    }
}
