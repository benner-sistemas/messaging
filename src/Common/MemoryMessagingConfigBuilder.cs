using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    /// <summary>
    /// Enum que representa os serviços de mensageria disponíveis.
    /// </summary>
    public enum Broker
    {
        ActiveMQ, AmazonSQS, AzureQueue, Rabbit
    }

    /// <summary>
    /// Classe construtora de um <see cref="IMessagingConfig"/> em memória.
    /// </summary>
    public class MemoryMessagingConfigBuilder
    {
        private readonly MemoryMessagingConfig _config;

        /// <summary>
        /// Instancia o builder com a configuração de fila padrão.
        /// </summary>
        /// <param name="defaultBrokerName">O nome do broker que será utilizado como padrão.</param>
        /// <param name="defaultBrokerConfigType">O serviço que será padrão.</param>
        public MemoryMessagingConfigBuilder(string defaultBrokerName, Broker defaultBrokerConfigType, Dictionary<string, string> configurations)
        {
            _config = new MemoryMessagingConfig(defaultBrokerName, GetBrokerConfigType(defaultBrokerConfigType), configurations);
        }

        public MemoryMessagingConfigBuilder WithQueue(string queueName, string brokerName)
        {
            _config.AddQueue(queueName, brokerName);
            return this;
        }

        public MemoryMessagingConfigBuilder WithQueues(Dictionary<string, string> queues)
        {
            foreach (var item in queues)
                WithQueue(item.Key, item.Value);
            return this;
        }

        public MemoryMessagingConfigBuilder WithBroker(string brokerName, Broker configType, Dictionary<string, string> configurations)
        {
            _config.AddBroker(brokerName, GetBrokerConfigType(configType), configurations);
            return this;
        }

        public MemoryMessagingConfig Create()
        {
            return _config;
        }

        private Type GetBrokerConfigType(Broker broker)
        {
            switch (broker)
            {
                case Broker.ActiveMQ:
                    return typeof(ActiveMQConfig);
                case Broker.AmazonSQS:
                    return typeof(AmazonSqsConfig);
                case Broker.AzureQueue:
                    return typeof(AzureMQConfig);
                case Broker.Rabbit:
                    return typeof(RabbitMQConfig);
                default:
                    return null;
            }
        }
    }
}