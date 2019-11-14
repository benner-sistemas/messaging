using System.Configuration;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe base da configuração customizada para filas.
    /// </summary>
    internal class MessagingFileConfigSection : ConfigurationSection
    {
        /// <summary>
        /// Nome da tag no arquivo xml.
        /// </summary>
        public const string SECTION_NAME = "MessagingConfigSection";

        /// <summary>
        /// A tag 'queues'.
        /// </summary>
        [ConfigurationProperty("queues")]
        internal QueueConfigCollection QueueList
        {
            get { return base["queues"] as QueueConfigCollection; }
        }

        /// <summary>
        /// A tag 'brokerList'.
        /// </summary>
        [ConfigurationProperty("brokerList")]
        internal BrokerListConfigCollection BrokerList
        {
            get
            {
                return base["brokerList"] as BrokerListConfigCollection;
            }
        }
    }
}