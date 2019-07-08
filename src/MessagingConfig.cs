using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    public class MessagingConfig : IMessagingConfig
    {
        /// <summary>
        /// {queueName, brokerName}
        /// </summary>
        private readonly Dictionary<string, string> _brokerNameByQueue;

        /// <summary>
        /// {brokerName, configurationType}
        /// </summary>
        private readonly Dictionary<string, Type> _brokerConfigTypesByBrokerName;

        /// <summary>
        /// {configurationType, settingsDictionary}
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, string>> _brokerSettingsByBrokerName;

        /// <summary>
        /// Default broker's name
        /// </summary>
        internal string Default { get; }

        internal MessagingConfig(string defaultBrokerName, Type defaultBrokerConfigType, Dictionary<string, string> configurations)
        {
            if (string.IsNullOrWhiteSpace(defaultBrokerName))
                throw new ArgumentException("Default broker name must be informed.", nameof(defaultBrokerName));

            if (defaultBrokerConfigType == null)
                throw new ArgumentNullException(nameof(defaultBrokerConfigType), "The default broker's type must be informed.");

            _brokerNameByQueue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _brokerConfigTypesByBrokerName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _brokerSettingsByBrokerName = new Dictionary<Type, Dictionary<string, string>>();
            Default = defaultBrokerName;
            configurations = new Dictionary<string, string>(configurations, StringComparer.OrdinalIgnoreCase);
            SetBroker(defaultBrokerName, defaultBrokerConfigType, configurations);
        }

        internal void SetBroker(string brokerName, Type brokerConfigType, Dictionary<string, string> brokerSettings)
        {
            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Broker name cannot be empty.", nameof(brokerName));

            brokerSettings = new Dictionary<string, string>(brokerSettings, StringComparer.OrdinalIgnoreCase);
            _brokerConfigTypesByBrokerName[brokerName] = brokerConfigType ?? throw new ArgumentNullException(nameof(brokerConfigType), "Broker configuration type must be informed.");
            _brokerSettingsByBrokerName[brokerConfigType] = brokerSettings ?? throw new ArgumentNullException(nameof(brokerSettings), "Broker configuration settings must be informed.");
        }

        internal void SetQueue(string queueName, string brokerName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Queue name must be informed.", nameof(queueName));

            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Broker name must be informed.", nameof(brokerName));

            Utils.ValidateQueueName(queueName, true);
            _brokerNameByQueue[queueName] = brokerName;
        }

        /// <summary>
        /// Gets the instance of an <see cref="IBrokerConfig"/> related to the queue name.
        /// If the queue doesn't exist in the configuration, the default broker will be used.
        /// </summary>
        public IBrokerConfig GetConfigForQueue(string queueName)
        {
            Type configType;
            Dictionary<string, string> configs;
            if (_brokerNameByQueue.ContainsKey(queueName))
            {
                string brokerName = _brokerNameByQueue[queueName];
                if (!_brokerConfigTypesByBrokerName.ContainsKey(brokerName))
                    throw new ArgumentException($"The broker with name \"{brokerName}\" could not be found.");

                configType = _brokerConfigTypesByBrokerName[brokerName];
                configs = _brokerSettingsByBrokerName[configType];
            }
            else
            {
                configType = _brokerConfigTypesByBrokerName[Default];
                configs = _brokerSettingsByBrokerName[configType];
                SetQueue(queueName, Default);
                SetBroker(Default, configType, configs);
            }

            return (IBrokerConfig)Activator.CreateInstance(configType, new object[] { configs });
        }
    }
}