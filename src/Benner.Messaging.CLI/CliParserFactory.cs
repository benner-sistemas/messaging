using Benner.Messaging.CLI.Verbs.Listener;
using Benner.Messaging.CLI.Verbs.Producer;

namespace Benner.Messaging.CLI
{
    /// <summary>
    /// Factory para criar um parser para linha de comando de acordo com quem está utilizando
    /// </summary>
    public static class CliParserFactory
    {
        public static CliParser CreateForListener(string[] args) => new CliParser(args, typeof(ProducerVerb));

        public static CliParser CreateForProducer(string[] args) => new CliParser(args, typeof(ListenerVerb));
    }
}
