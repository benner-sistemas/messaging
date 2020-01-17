using Benner.Messaging.Interfaces;
using CommandLine;

namespace Benner.Messaging.CLI.Verbs.Listener
{
    [Verb("azure", HelpText = "Iniciar um listener para Azuere Queue")]
    public class AzureVerb : ListenerVerb
    {
        [Option('c', "connectionString", HelpText = "A string de conexão com o serviço Azure.", Required = true)]
        public string ConnectionString { get; set; }

        [Option('i', "invisibilityTime", HelpText = "O tempo que a mensagem permanecerá invisível para outras filas, em segundos.", Required = true)]
        public int InvisibilityTime { get; set; }

        public override string BrokerName => "AzureQueue";

        public override IMessagingConfig GetConfiguration()
        {
            ValidateParameters();

            return new MessagingConfigBuilder()
                .WithAzureQueueBroker("azure", ConnectionString, InvisibilityTime, setAsDefault: true)
                .Create();
        }

        public override void ValidateParameters()
        {
            OptionValidator.ValidateOption("-n/--consumerName", Consumer);
            OptionValidator.ValidateOption("-i/--invisibilityTime", InvisibilityTime);
            OptionValidator.ValidateOption("-c/--connectionString", ConnectionString);
        }
    }
}
