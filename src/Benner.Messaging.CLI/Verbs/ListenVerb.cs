using Benner.Messaging.CLI.Attributes;
using Benner.Messaging.Interfaces;
using CommandLine;
using System;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("listen", HelpText = "Começa a escutar uma fila")]
    [SubVerbs(typeof(ActiveVerb), typeof(AmazonVerb), typeof(AzureVerb), typeof(RabbitVerb))]
    public abstract class ListenVerb
    {
        [Option('n', "consumerName", HelpText = "O nome completo da classe Consumer que será utilizada. 'Namespace1.Namespace2.Classe'.", Required = true)]
        public string Consumer { get; protected set; }

        public virtual string BrokerName { get; }

        public abstract IMessagingConfig GetConfiguration();

        protected void ValidateOption(string parName, object value)
        {
            if (value is string valor && string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"O parâmetro '{parName}' deve ser informado.");

            if (value is int num && num <= 0)
                throw new ArgumentException($"O parâmetro '{parName}' deve ser maior que 0.");
        }
    }
}
