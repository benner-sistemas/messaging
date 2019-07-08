using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    /// <summary>
    /// Class used to build and create the instance of an in-memory <see cref="IMessagingConfig"/>.
    /// </summary>
    public class MessagingConfigBuilder
    {
        private readonly MessagingConfig _config;

        /// <summary>
        /// Instantiates the builder with default configuration already
        /// </summary>
        /// <param name="defaultBrokerName">The broker name to set as default broker.</param>
        /// <param name="defaultBrokerConfigType">The broker service to set as default.</param>
        public MessagingConfigBuilder(string defaultBrokerName, BrokerType defaultBrokerConfigType, Dictionary<string, string> configurations)
        {
            _config = new MessagingConfig(defaultBrokerName, GetBrokerConfigType(defaultBrokerConfigType), configurations);
        }

        /// <summary>
        /// Sets an ActiveMQ broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithActiveMQBroker(string brokerName, string hostName, int port = 61616, string userName = "admin", string password = "admin")
        {
            return WithBroker(brokerName, BrokerType.ActiveMQ,
             new Dictionary<string, string>
                 {
                        {"HostName", hostName},
                        {"UserName", userName},
                        {"Password", password},
                        {"Port", port.ToString()}
                 });
        }

        /// <summary>
        /// Sets an AmazonSQS broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithAmazonSQSBroker(string brokerName, int invisibilityTime)
        {
            return WithBroker(brokerName, BrokerType.AmazonSQS,
             new Dictionary<string, string>
                 {
                        {"InvisibilityTime", invisibilityTime.ToString()}
                 });
        }

        /// <summary>
        /// Sets an Azure Queue broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithAzureQueueBroker(string brokerName, string connectionString, int invisibilityTime)
        {
            return WithBroker(brokerName, BrokerType.AzureQueue,
              new Dictionary<string, string>
                  {
                        {"InvisibilityTime", invisibilityTime.ToString()},
                        {"ConnectionString", connectionString}
                  });
        }

        /// <summary>
        /// Sets an RabbitMQ broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithRabbitMQBroker(string brokerName, string hostName, int port = 5672, string userName = "guest", string password = "guest")
        {
            return WithBroker(brokerName, BrokerType.RabbitMQ,
                new Dictionary<string, string>
                    {
                        {"HostName", hostName},
                        {"UserName", userName},
                        {"Password", password},
                        {"Port", port.ToString()}
                    });
        }

        /// <summary>
        /// Sets any broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithBroker(string brokerName, BrokerType configType, Dictionary<string, string> configurations)
        {
            _config.SetBroker(brokerName, GetBrokerConfigType(configType), configurations);
            return this;
        }

        /// <summary>
        /// Sets a queue with a name and broker name that needs to be set.
        /// </summary>
        public MessagingConfigBuilder WithMappedQueue(string queueName, string brokerName)
        {
            _config.SetQueue(queueName, brokerName);
            return this;
        }

        public MessagingConfig Create() => _config;

        private Type GetBrokerConfigType(BrokerType broker)
        {
            switch (broker)
            {
                case BrokerType.ActiveMQ:
                    return typeof(ActiveMQConfig);
                case BrokerType.AmazonSQS:
                    return typeof(AmazonSQSConfig);
                case BrokerType.AzureQueue:
                    return typeof(AzureQueueConfig);
                case BrokerType.RabbitMQ:
                    return typeof(RabbitMQConfig);
                default:
                    return null;
            }
        }
    }
}