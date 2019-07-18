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

        public MessagingConfigBuilder() => _config = new MessagingConfig();

        /// <summary>
        /// Instantiates the builder with default configuration already
        /// </summary>
        /// <param name="defaultBrokerName">The broker name to set as default broker.</param>
        /// <param name="defaultBrokerConfigType">The broker service to set as default.</param>
        /// <param name="defaultBrokerSettings"></param>
        public MessagingConfigBuilder(string defaultBrokerName, BrokerType defaultBrokerConfigType, Dictionary<string, string> defaultBrokerSettings)
        {
            _config = new MessagingConfig(defaultBrokerName, GetBrokerConfigType(defaultBrokerConfigType), defaultBrokerSettings);
        }

        /// <summary>
        /// Sets an ActiveMQ broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithActiveMQBroker(string brokerName, string hostName, int port = 61616, string userName = "admin", string password = "admin", bool setAsDefault = false)
        {
            return WithBroker(brokerName, BrokerType.ActiveMQ,
                new Dictionary<string, string>
                {
                    {"HostName", hostName},
                    {"UserName", userName},
                    {"Password", password},
                    {"Port", port.ToString()}
                },
                setAsDefault);
        }

        /// <summary>
        /// Sets an AmazonSQS broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithAmazonSQSBroker(string brokerName, int invisibilityTime, bool setAsDefault = false)
        {
            return WithBroker(brokerName, BrokerType.AmazonSQS,
                new Dictionary<string, string>
                {
                    { "InvisibilityTime", invisibilityTime.ToString()}
                },
                setAsDefault);
        }

        /// <summary>
        /// Sets an Azure Queue broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithAzureQueueBroker(string brokerName, string connectionString, int invisibilityTime, bool setAsDefault = false)
        {
            return WithBroker(brokerName, BrokerType.AzureQueue,
                new Dictionary<string, string>
                {
                    { "InvisibilityTime", invisibilityTime.ToString()},
                    {"ConnectionString", connectionString}
                },
                setAsDefault);
        }

        /// <summary>
        /// Sets an RabbitMQ broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithRabbitMQBroker(string brokerName, string hostName, int port = 5672, string userName = "guest", string password = "guest", bool setAsDefault = false)
        {
            return WithBroker(brokerName, BrokerType.RabbitMQ,
                new Dictionary<string, string>
                {
                    {"HostName", hostName},
                    {"UserName", userName},
                    {"Password", password},
                    {"Port", port.ToString()}
                },
                setAsDefault);
        }

        /// <summary>
        /// Sets any broker in the in-memory configuration.
        /// </summary>
        public MessagingConfigBuilder WithBroker(string brokerName, BrokerType configType, Dictionary<string, string> configurations, bool setAsDefault = false)
        {
            _config.SetBroker(brokerName, GetBrokerConfigType(configType), configurations);
            if (setAsDefault)
                _config.DefaultBrokerName = brokerName;
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

        public MessagingConfig Create()
        {
            if (string.IsNullOrEmpty(_config.DefaultBrokerName))
                throw new InvalidOperationException(ErrorMessages.DefaultBrokerNotFound);

            return this._config;
        }

        private Type GetBrokerConfigType(BrokerType brokerType)
        {
            switch (brokerType)
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