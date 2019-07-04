using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    public class MessagingConfigFactory
    {
        private readonly MessagingConfig _config = new MessagingConfig();

        public static MessagingConfigFactory NewMessagingConfigFactory()
        {
            return new MessagingConfigFactory();
        }

        public MessagingConfigFactory WithRabbitMQBroker(string hostName, int port, string userName, string password)
        {
            return WithRabbitMQBroker(
                new Dictionary<string, string> {
                    { "HostName",   hostName },
                    { "Port",       port.ToString() },
                    { "UserName",   userName },
                    { "Password",   password },
                });
        }

        public MessagingConfigFactory WithRabbitMQBroker(Dictionary<string, string> brokerSettings)
        {
            _config.SetBroker(
                BrokerType.RabbitMQ.ToString(),
                GetBrokerConfigType(BrokerType.RabbitMQ),
                brokerSettings);

            return this;
        }

        public MessagingConfigFactory WithActiveMQBroker(Dictionary<string, string> brokerSettings)
        {
            _config.SetBroker(
                BrokerType.ActiveMQ.ToString(),
                GetBrokerConfigType(BrokerType.ActiveMQ),
                brokerSettings);

            return this;
        }

        public MessagingConfigFactory WithAmazonSQSBroker(Dictionary<string, string> brokerSettings)
        {
            _config.SetBroker(
                BrokerType.AmazonSQS.ToString(),
                GetBrokerConfigType(BrokerType.AmazonSQS),
                brokerSettings);

            return this;
        }
        public MessagingConfigFactory WithAzureQueueBroker(Dictionary<string, string> brokerSettings)
        {
            _config.SetBroker(
                BrokerType.AzureQueue.ToString(),
                GetBrokerConfigType(BrokerType.AzureQueue),
                brokerSettings);

            return this;
        }
        

        public MessagingConfigFactory WithMappedQueue(string queueName, string brokerName)
        {
            _config.SetQueue(queueName, brokerName);

            return this;
        }

        public IMessagingConfig Create()
        {
            return _config;
        }

        private Type GetBrokerConfigType(BrokerType broker)
        {
            switch (broker)
            {
                case BrokerType.ActiveMQ:
                    return typeof(ActiveMQConfig);
                case BrokerType.AmazonSQS:
                    return typeof(AmazonSQSConfig);
                case BrokerType.AzureQueue:
                    return typeof(AzureMQConfig);
                case BrokerType.RabbitMQ:
                    return typeof(RabbitMQConfig);
                default:
                    return null;
            }
        }
    }
}