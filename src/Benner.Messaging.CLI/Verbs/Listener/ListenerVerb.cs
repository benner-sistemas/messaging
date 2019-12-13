using Benner.Messaging.CLI.Attributes;
using Benner.Messaging.Interfaces;
using CommandLine;

namespace Benner.Messaging.CLI.Verbs.Listener
{
    [Verb("listen", HelpText = "Começa a escutar uma fila")]
    [SubVerbs(typeof(ActiveVerb), typeof(AmazonVerb), typeof(AzureVerb), typeof(RabbitVerb))]
    public abstract class ListenerVerb : IBrokerVerb
    {
        [Option('n', "consumerName", HelpText = "O nome completo da classe Consumer que será utilizada. 'Namespace1.Namespace2.Classe'.", Required = true)]
        public string Consumer { get; protected set; }

        public virtual string BrokerName { get; }

        public abstract IMessagingConfig GetConfiguration();
    }
}
