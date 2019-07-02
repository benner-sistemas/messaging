using System;
using System.Collections.Generic;
using Benner.Messaging.Interfaces;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe responsável pela configuração das filas em memória.
    /// </summary>
    public class MemoryMessagingConfig : IMessagingConfig
    {
        /// <summary>
        /// key: queueName, 
        /// value: brokerName
        /// </summary>
        private readonly Dictionary<string, string> _queues;

        /// <summary>
        /// key: brokerName, 
        /// value: type da configuração
        /// </summary>
        private readonly Dictionary<string, Type> _configurations;

        /// <summary>
        /// key: type da configuração, 
        /// value: dicionario das configs
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, string>> _configurationsForBroker;

        /// <summary>
        /// Nome do broker default
        /// </summary>
        internal string Default { get; }

        internal MemoryMessagingConfig(string defaultBrokerName, Type defaultBrokerConfigType, Dictionary<string, string> configurations)
        {
            if (string.IsNullOrWhiteSpace(defaultBrokerName))
                throw new ArgumentException("Nome do broker default deve ser informado.", nameof(defaultBrokerName));

            if (defaultBrokerConfigType == null)
                throw new ArgumentNullException(nameof(defaultBrokerConfigType), "Type da configuração deve ser informado.");

            _queues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _configurations = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _configurationsForBroker = new Dictionary<Type, Dictionary<string, string>>();
            Default = defaultBrokerName;
            configurations = new Dictionary<string, string>(configurations, StringComparer.OrdinalIgnoreCase);
            AddBroker(defaultBrokerName, defaultBrokerConfigType, configurations);
        }

        internal void AddBroker(string brokerName, Type brokerConfigType, Dictionary<string, string> configurations)
        {
            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Nome do broker não deve ser vazio.", nameof(brokerName));

            if (brokerConfigType == null)
                throw new ArgumentNullException(nameof(brokerConfigType), "Type da configuração deve ser informado.");

            if (configurations == null)
                throw new ArgumentNullException(nameof(configurations), "Dicionário de configurações deve ser informado.");

            configurations = new Dictionary<string, string>(configurations, StringComparer.OrdinalIgnoreCase);
            _configurations[brokerName] = brokerConfigType;
            _configurationsForBroker[brokerConfigType] = configurations;
        }

        internal void AddQueue(string queueName, string brokerName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
                throw new ArgumentException("Nome da fila não deve ser vazio.", nameof(queueName));

            if (string.IsNullOrWhiteSpace(brokerName))
                throw new ArgumentException("Nome do broker não deve ser vazio.", nameof(brokerName));

            Utils.ValidateQueueName(queueName, true);
            _queues[queueName] = brokerName;
        }

        /// <summary>
        /// Obtém a instância de um <see cref="IBrokerConfig"/> relativo ao nome da fila. 
        /// Caso a fila não exista na configuração, o broker definido em <see cref="Default"/> é utilizado.
        /// </summary>
        /// <param name="queueName">Nome da fila</param>
        /// <returns>A instância de um <see cref="IBrokerConfig"/></returns>
        public IBrokerConfig GetConfigForQueue(string queueName)
        {
            Type configType;
            Dictionary<string, string> configs;
            if (_queues.ContainsKey(queueName))
            {
                string brokerName = _queues[queueName];
                if (!_configurations.ContainsKey(brokerName))
                    throw new ArgumentException($"O broker de nome \"{brokerName}\" não foi encontrado.");

                configType = _configurations[brokerName];
                configs = _configurationsForBroker[configType];
            }
            else
            {
                configType = _configurations[Default];
                configs = _configurationsForBroker[configType];
                AddQueue(queueName, Default);
                AddBroker(Default, configType, configs);
            }
            return (IBrokerConfig)Activator.CreateInstance(configType, new object[] { configs });
        }
    }
}