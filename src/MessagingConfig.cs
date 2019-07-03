using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Benner.Messaging
{
    public class MessagingConfig : IMessagingConfig
    {
        public IBrokerConfig GetConfigForQueue(string queueName)
        {
            string brokerName;
            if (!_brokerNameByQueue.TryGetValue(queueName, out brokerName))
            {
                if (_brokerConfigTypesByBrokerName.Count == 0)
                    throw new ArgumentException($"Broker config not found");

                brokerName = _brokerConfigTypesByBrokerName.First().Key;
            }

            Type brokerConfigType;
            if (!_brokerConfigTypesByBrokerName.TryGetValue(brokerName, out brokerConfigType))
                throw new ArgumentException($"Broker config type '{brokerName}' not found");

            var brokerConfigSettings = _brokerSettingsByBrokerName[brokerName];

            return (IBrokerConfig)Activator.CreateInstance(brokerConfigType, new object[] { brokerConfigSettings });
        }



        /// <summary>
        /// key: queueName, 
        /// value: brokerName
        /// </summary>
        private readonly Dictionary<string, string> _brokerNameByQueue = new Dictionary<string, string>();

        /// <summary>
        /// key: brokerName, 
        /// value: type da configuração
        /// </summary>
        private readonly Dictionary<string, Type> _brokerConfigTypesByBrokerName = new Dictionary<string, Type>();

        /// <summary>
        /// key: type da configuração, 
        /// value: dicionario das configs
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, string>> _brokerSettingsByBrokerName = new Dictionary<string, Dictionary<string, string>>();

        internal void SetBroker(string brokerName, Type brokerConfigType, Dictionary<string, string> brokerSettings)
        {
            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Broker name must be informed", nameof(brokerName));

            _brokerConfigTypesByBrokerName[brokerName] = brokerConfigType ?? throw new ArgumentNullException(nameof(brokerConfigType), "Broker config type must be informed");
            _brokerSettingsByBrokerName[brokerName] = brokerSettings ?? throw new ArgumentNullException(nameof(brokerSettings), "Broker config settings must be informed");
        }

        internal void SetQueue(string queueName, string brokerName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be informed", nameof(queueName));

            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Broker name must be informed", nameof(brokerName));

            Utils.ValidateQueueName(queueName, true);
            _brokerNameByQueue[queueName] = brokerName;
        }
    }
}