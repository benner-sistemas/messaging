using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;

namespace Benner.Messaging
{
    /// <summary>
    /// Class responsible for configuring queues from a .config file
    /// </summary>
    public class FileMessagingConfig : IMessagingConfig
    {
        /// <summary>
        /// key: queueName
        /// value: brokerName
        /// </summary>
        private readonly Dictionary<string, string> _queues;

        /// <summary>
        /// key: brokerName
        /// value: configuração>
        /// </summary>
        private readonly Dictionary<string, IBrokerConfig> _configurations;

        private readonly MessagingFileConfigSection _messagingConfig;
        private static string _defaultConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "messaging.config");

        public static bool DefaultConfigFileExists
        {
            get
            {
                return File.Exists(_defaultConfigPath);
            }
        }

        /// <summary>
        /// Instantiates the broker configurations through the 'messaging.config' file 
        /// that exists in the same directory of the executing assembly.
        /// The file structure is fully validated in the creation.
        /// </summary>
        public FileMessagingConfig() : this(_defaultConfigPath)
        { }

        /// <summary>
        /// Instantiates the broker configurations through a file.
        /// The file structure is fully validated in the creation.
        /// </summary>
        /// <param name="fileName">The file's full path</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ConfigurationErrorsException"></exception>
        public FileMessagingConfig(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"The messaging.config file with path '{fileName}' could not be found.", fileName);

            var map = new ExeConfigurationFileMap() { ExeConfigFilename = fileName };
            var configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            _messagingConfig = configManager.GetSection(MessagingFileConfigSection.SECTION_NAME) as MessagingFileConfigSection;

            ValidateConfigFile();

            _queues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _configurations = new Dictionary<string, IBrokerConfig>(StringComparer.OrdinalIgnoreCase);
        }

        private void ValidateConfigFile()
        {
            // Checks if there are brokers
            if (_messagingConfig.BrokerList.Count == 0)
                throw new XmlException("No broker could be found inside brokerList.");

            // Checks for 'add' tags inside all brokers
            foreach (var item in _messagingConfig.BrokerList.Brokers)
                if (item.Count == 0)
                    throw new XmlException($"No specific configurations found for broker \"{item.Name}\".");

            // Checks pre configured queue names to see if they're valid according to the rules
            foreach (var queueName in _messagingConfig.QueueList.Queues.Select(n => n.Name))
                Utils.ValidateQueueName(queueName, true);

            // Checks default's existence
            var defaultName = _messagingConfig.BrokerList.Default;
            var defaultBroker = _messagingConfig.BrokerList.Brokers.FirstOrDefault(b => b.Name.Equals(defaultName, StringComparison.OrdinalIgnoreCase))
                  ?? throw new XmlException("The broker set as default could not be found in the configuration file.");
        }

        /// <summary>
        /// Gets an instance for the informed queue's broker configuration
        /// </summary>
        private IBrokerConfig GetTransporterConfigInstance(string queueName, out string brokerName)
        {
            var brokerList = _messagingConfig.BrokerList;
            BrokerConfigCollection broker;
            var queue = _messagingConfig.QueueList[queueName];

            if (queue != null)
            {
                brokerName = queue.Broker;
                broker = brokerList[brokerName] ?? throw new ArgumentException($"The broker with name \"{brokerName}\" could not be found.");
            }
            // If queue doesn't exist, uses default 
            else
            {
                brokerName = brokerList.Default;
                broker = brokerList[brokerName] ?? throw new ArgumentException($"The default broker's configuration with name \"{brokerName}\" could not be found.");
                _messagingConfig.QueueList.Add(new QueueConfigElement(queueName, brokerName));
            }

            var configs = broker.GetEnumerable().ToDictionary(key => key.Key, value => value.Value, StringComparer.OrdinalIgnoreCase);
            return (IBrokerConfig)Activator.CreateInstance(broker.BrokerType, new object[] { configs });
        }

        /// <summary>
        /// Gets the instance of a <see cref="IBrokerConfig"/> related to the queue name.
        /// If the queue does not exist in the configuration, the broker set as default is used.
        /// </summary>
        public IBrokerConfig GetConfigForQueue(string queueName)
        {
            if (_queues.ContainsKey(queueName))
                return _configurations[_queues[queueName]];

            var configInstance = GetTransporterConfigInstance(queueName, out string brokerName);

            _queues[queueName] = brokerName;
            _configurations[brokerName] = configInstance;

            return configInstance;
        }

        /// <summary>
        /// Gets the broker name configured for a specific queue name. Returns null if the queue is not found.
        /// </summary>
        /// <param name="queue"></param>
        /// <returns></returns>
        public string GetBrokerNameForQueue(string queue)
        {
            return _messagingConfig.QueueList.Queues.FirstOrDefault(q => q.Name == queue)?.Broker;
        }
    }
}