using System;
using System.Collections.Generic;
using System.Configuration;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração da tag 'brokerList'. Atributo chave: <see cref="Default"/>
    /// </summary>
    [ConfigurationCollection(typeof(BrokerConfigCollection), AddItemName = "broker", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal class BrokerListConfigCollection : ConfigurationElementCollection
    {
        internal BrokerListConfigCollection() : base(StringComparer.OrdinalIgnoreCase)
        { }

        /// <summary>
        /// O atributo 'default' da tag. É a chave da tag.
        /// </summary>
        [ConfigurationProperty("default", IsKey = true, IsRequired = true)]
        internal string Default
        {
            get
            {
                return base["default"] as string;
            }
        }

        /// <summary>
        /// Indexador para as tags 'broker' filhas.
        /// </summary>
        /// <param name="key">O valor do atributo 'name' das tags 'broker' filhas.</param>
        /// <returns>O elemento 'broker' encontrado.</returns>
        internal new BrokerConfigCollection this[string key]
        {
            get
            {
                return (BrokerConfigCollection)BaseGet(key);
            }
        }

        internal IEnumerable<BrokerConfigCollection> Brokers
        {
            get
            {
                foreach (var key in BaseGetAllKeys())
                    yield return BaseGet(key) as BrokerConfigCollection;
            }
        }

        protected override ConfigurationElement CreateNewElement() => new BrokerConfigCollection();

        protected override object GetElementKey(ConfigurationElement element) => ((BrokerConfigCollection)element).Name;
    }
}