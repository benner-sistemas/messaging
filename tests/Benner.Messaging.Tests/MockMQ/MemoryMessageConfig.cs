using System;
using System.Collections.Generic;
using Benner.Messaging.Interfaces;

namespace Benner.Messaging.Tests
{
    public class MemoryMessageConfig : IMessagingConfig
    {
        /// <summary>
        /// key: queueName
        /// value: brokerName
        /// </summary>
        private readonly Dictionary<string, string> _queues;

        /// <summary>
        /// key: brokerName
        /// value: type da configuração>
        /// </summary>
        private readonly Dictionary<string, Type> _configurations;

        private readonly string _default;

        public MemoryMessageConfig(string defaultBroker, Type defaultBrokerConfigType)
        {
            _queues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _configurations = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _default = defaultBroker;
            WithBroker(defaultBroker, defaultBrokerConfigType);
        }

        public MemoryMessageConfig WithQueue(string queueName, string brokerName)
        {
            _queues[queueName] = brokerName;
            return this;
        }

        public MemoryMessageConfig WithBroker(string brokerName, Type configType)
        {
            _configurations[brokerName] = configType;
            return this;
        }

        public IBrokerConfig GetConfigForQueue(string queueName)
        {
            Type configType;
            string brokerName;
            if (_queues.ContainsKey(queueName) && _configurations.ContainsKey(_queues[queueName]))
            {
                brokerName = _queues[queueName];
                configType = _configurations[brokerName];
            }
            else
            {
                brokerName = _default;
                configType = _configurations[_default];
                WithQueue(queueName, brokerName).WithBroker(brokerName, configType);
            }
            return (IBrokerConfig)Activator.CreateInstance(configType, new object[] { null });
        }
    }
}
