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
        private readonly Dictionary<string, string> _brokerNameByQueue = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// {brokerName, configurationType}
        /// </summary>
        private readonly Dictionary<string, Type> _brokerConfigTypesByBrokerName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// {configurationType, settingsDictionary}
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, string>> _brokerSettingsByBrokerName = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Default broker's name
        /// </summary>
        internal string DefaultBrokerName { get; set; }

        internal MessagingConfig()
        {
        }

        internal MessagingConfig(string defaultBrokerName, Type defaultBrokerConfigType, Dictionary<string, string> defaultBrokerSettings)
        {
            if (string.IsNullOrWhiteSpace(defaultBrokerName))
                throw new ArgumentException("Default broker name must be informed.", nameof(defaultBrokerName));

            if (defaultBrokerConfigType == null)
                throw new ArgumentNullException(nameof(defaultBrokerConfigType), "The default broker's type must be informed.");

            DefaultBrokerName = defaultBrokerName;
            SetBroker(defaultBrokerName, defaultBrokerConfigType, defaultBrokerSettings);
        }

        internal void SetBroker(string brokerName, Type brokerConfigType, Dictionary<string, string> brokerSettings)
        {
            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Broker name cannot be empty.", nameof(brokerName));

            _brokerConfigTypesByBrokerName[brokerName] = brokerConfigType ?? throw new ArgumentNullException(nameof(brokerConfigType), "Broker configuration type must be informed.");
            _brokerSettingsByBrokerName[brokerName] = brokerSettings ?? throw new ArgumentNullException(nameof(brokerSettings), "Broker configuration settings must be informed.");
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
            string brokerName;
            if (!_brokerNameByQueue.TryGetValue(queueName, out brokerName))
                brokerName = DefaultBrokerName;

            Type brokerConfigType;
            if (!_brokerConfigTypesByBrokerName.TryGetValue(brokerName, out brokerConfigType))
                throw new ArgumentException($"The broker with name \"{brokerName}\" could not be found.");

            var brokerSettings = _brokerSettingsByBrokerName[brokerName];
            return (IBrokerConfig)Activator.CreateInstance(brokerConfigType, new object[] { brokerSettings });
        }
    }
}