using System.Configuration;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração das tags 'add'.  Atributo chave: <see cref="Key"/>
    /// </summary>
    internal class AddConfigElement : ConfigurationElement
    {
        /// <summary>
        /// O atributo 'key' da tag. É a chave da tag.
        /// </summary>
        [ConfigurationProperty("key", IsKey = true, IsRequired = true)]
        internal string Key
        {
            get
            {
                return base["key"] as string;
            }
        }

        /// <summary>
        /// O atributo 'value' da tag.
        /// </summary>
        [ConfigurationProperty("value", IsRequired = true)]
        internal string Value
        {
            get
            {
                return base["value"] as string;
            }
        }
    }
}
