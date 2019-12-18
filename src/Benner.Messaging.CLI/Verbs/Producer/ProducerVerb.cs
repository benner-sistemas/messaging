using Benner.Messaging.CLI.Attributes;
using Benner.Messaging.Interfaces;
using CommandLine;

namespace Benner.Messaging.CLI.Verbs.Producer
{
    [Verb("produce", HelpText = "Configura uma fila para produzir")]
    [SubVerbs(typeof(ActiveVerb), typeof(AmazonVerb), typeof(AzureVerb), typeof(RabbitVerb))]
    public abstract class ProducerVerb : IBrokerVerb
    {
        public virtual string BrokerName { get; }

        public abstract IMessagingConfig GetConfiguration();

        public abstract void ValidateParameters();
    }
}
