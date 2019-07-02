using System.Configuration;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração das tags 'queue'.  Atributo chave: <see cref="Name"/>
    /// </summary>
    internal class QueueConfigElement : ConfigurationElement
    {
        internal QueueConfigElement()
        { }

        internal QueueConfigElement(string name, string brokerName)
        {
            Name = name;
            Broker = brokerName;
        }

        /// <summary>
        /// O atributo 'name' da tag. É a chave da tag.
        /// </summary>
        [ConfigurationProperty("name")]
        internal string Name
        {
            get
            {
                return (base["name"] as string);
            }
            set
            {
                this["name"] = value;
            }
        }

        /// <summary>
        /// O atributo 'broker' da tag.
        /// </summary>
        [ConfigurationProperty("broker")]
        internal string Broker
        {
            get
            {
                return base["broker"] as string;
            }
            set
            {
                this["broker"] = value;
            }
        }
    }
}
