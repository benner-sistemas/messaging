using System;
using System.Collections.Generic;
using System.Configuration;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração da tag 'queues'.
    /// </summary>
    [ConfigurationCollection(typeof(QueueConfigElement), AddItemName = "queue", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal class QueueConfigCollection : ConfigurationElementCollection
    {
        public QueueConfigCollection() : base(StringComparer.OrdinalIgnoreCase)
        { }

        /// <summary>
        /// Indexador para as tags 'queue' filhas.
        /// </summary>
        /// <param name="key">O valor do atributo 'name' das tags 'queue' filhas.</param>
        /// <returns>O elemento 'queue' encontrado.</returns>
        internal new QueueConfigElement this[string key]
        {
            get
            {
                return (QueueConfigElement)BaseGet(key);
            }
        }

        internal IEnumerable<QueueConfigElement> Queues
        {
            get
            {
                foreach (var key in BaseGetAllKeys())
                    yield return BaseGet(key) as QueueConfigElement;
            }
        }

        /// <summary>
        /// Adiciona uma tag 'queue' na configuração.
        /// </summary>
        /// <param name="queueConfig"></param>
        internal void Add(QueueConfigElement queueConfig) => BaseAdd(queueConfig);

        protected override ConfigurationElement CreateNewElement() => new QueueConfigElement();

        protected override object GetElementKey(ConfigurationElement element) => ((QueueConfigElement)element).Name;
    }
}