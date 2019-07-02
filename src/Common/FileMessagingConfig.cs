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
    /// Classe responsável pela configuração das filas em formato de arquivo .config.
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

        /// <summary>
        /// Instancia a configuração de mensageria através de um arquivo 'messaging.config' 
        /// presente no mesmo diretório do assembly em execução.
        /// O arquivo é validado na inicialização da instância.
        /// </summary>
        public FileMessagingConfig() : this(Path.Combine(Directory.GetCurrentDirectory(), "messaging.config"))
        { }

        /// <summary>
        /// Instancia a configuração de mensageria através de arquivo.
        /// O arquivo é validado na inicialização da instância.
        /// </summary>
        /// <param name="fileName">O caminho completo do arquivo.</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ConfigurationErrorsException"></exception>
        public FileMessagingConfig(string fileName)
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException($"Arquivo de configuração '{fileName}' não encontrado.");

            var map = new ExeConfigurationFileMap() { ExeConfigFilename = fileName };
            var configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            _messagingConfig = configManager.GetSection(MessagingFileConfigSection.SECTION_NAME) as MessagingFileConfigSection;

            ValidateConfigFile();

            _queues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _configurations = new Dictionary<string, IBrokerConfig>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Faz validações extras específicas
        /// </summary>
        private void ValidateConfigFile()
        {
            // Validar se há brokers
            if (_messagingConfig.BrokerList.Count == 0)
                throw new XmlException("Nenhum broker encontrado na configuração.");

            // Validar que existem 'add's nos brokers
            foreach (var item in _messagingConfig.BrokerList.Brokers)
                if (item.Count == 0)
                    throw new XmlException($"Nenhuma configuração para o broker \"{item.Name}\" encontrada");

            // Validar nomes das queues no arquivo
            foreach (var queueName in _messagingConfig.QueueList.Queues.Select(n => n.Name))
                Utils.ValidateQueueName(queueName, true);

            // Validar se o default existe
            var defaultName = _messagingConfig.BrokerList.Default;
            var defaultBroker = _messagingConfig.BrokerList.Brokers.FirstOrDefault(b => b.Name.Equals(defaultName, StringComparison.OrdinalIgnoreCase))
                  ?? throw new XmlException("O broker definido como default não foi encontrado na configuração.");
        }

        /// <summary>
        /// Obtem uma instância para a configuração do broker da fila informada
        /// </summary>
        /// <param name="queueName">Nome da fila</param>
        /// <param name="brokerName">Nome do broker</param>
        /// <returns></returns>
        private IBrokerConfig GetTransporterConfigInstance(string queueName, out string brokerName)
        {
            var brokerList = _messagingConfig.BrokerList;
            BrokerConfigCollection broker;
            var queue = _messagingConfig.QueueList[queueName];

            //Se não existe, usa o default e adiciona no arquivo
            if (queue != null)
            {
                brokerName = queue.Broker;
                broker = brokerList[brokerName] ?? throw new ArgumentException($"O broker de nome \"{brokerName}\" não foi encontrado.");
            }
            else
            {
                brokerName = brokerList.Default;
                broker = brokerList[brokerName] ?? throw new ArgumentException($"A configuração do broker default \"{brokerName}\" não foi encontrada.");
                _messagingConfig.QueueList.Add(new QueueConfigElement(queueName, brokerName));
            }

            var configs = broker.GetEnumerable().ToDictionary(key => key.Key, value => value.Value, StringComparer.OrdinalIgnoreCase);
            return (IBrokerConfig)Activator.CreateInstance(broker.BrokerType, new object[] { configs });
        }

        /// <summary>
        /// Obtém a instância de um <see cref="IBrokerConfig"/> relativo ao nome da fila. 
        /// Caso a fila não exista na configuração, o broker definido como default em 'brokerList' é utilizado.
        /// </summary>
        /// <param name="queueName">O nome da fila</param>
        /// <returns>A instância de um <see cref="IBrokerConfig"/></returns>
        public IBrokerConfig GetConfigForQueue(string queueName)
        {
            if (_queues.ContainsKey(queueName))
                return _configurations[_queues[queueName]];

            var configInstance = GetTransporterConfigInstance(queueName, out string brokerName);

            _queues[queueName] = brokerName;
            _configurations[brokerName] = configInstance;

            return configInstance;
        }
    }
}