using System;
using System.Collections.Generic;
using System.Configuration;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração das tags 'broker'. Atributo chave: <see cref="Name"/>
    /// </summary>
    [ConfigurationCollection(typeof(AddConfigElement), AddItemName = "add", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    internal class BrokerConfigCollection : ConfigurationElementCollection
    {
        public BrokerConfigCollection() : base(StringComparer.OrdinalIgnoreCase)
        { }

        /// <summary>
        /// Indexador para as tags 'add' filhas.
        /// </summary>
        /// <param name="key">O valor do atributo 'key' das tags 'add' filhas.</param>
        /// <returns>O elemento 'add' encontrado.</returns>
        internal new AddConfigElement this[string key]
        {
            get
            {
                return (AddConfigElement)BaseGet(key);
            }
        }

        /// <summary>
        /// O atributo 'name' da tag. É a chave da tag.
        /// </summary>
        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        internal string Name
        {
            get
            {
                return base["name"] as string;
            }
        }

        /// <summary>
        /// O atributo 'type' da tag. Retorna o <see cref="System.Type"/> da classe de configuração do transporter.
        /// </summary>
        internal Type BrokerType
        {
            get
            {
                return System.Type.GetType(this.Type);
            }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        private string Type
        {
            get
            {
                return base["type"] as string;
            }
        }

        protected override ConfigurationElement CreateNewElement() => new AddConfigElement();

        protected override object GetElementKey(ConfigurationElement element) => ((AddConfigElement)element).Key;

        internal new IEnumerator<AddConfigElement> GetEnumerator()
        {
            int count = base.Count;
            for (int i = 0; i < count; i++)
                yield return base.BaseGet(i) as AddConfigElement;
        }

        internal IEnumerable<AddConfigElement> GetEnumerable()
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }
}