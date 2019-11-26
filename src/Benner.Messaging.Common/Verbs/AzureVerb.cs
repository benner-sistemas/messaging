using Benner.Messaging.Interfaces;
using CommandLine;

namespace Benner.Messaging.Common.Verbs
{
    [Verb("azure", HelpText = "Iniciar um listener para Azuere Queue")]
    public class AzureVerb : ListenVerb
    {
        [Option('c', "connectionString", HelpText = "A string de conexão com o serviço Azure.", Required = true)]
        public string ConnectionString { get; set; }

        [Option('i', "invisibilityTime", HelpText = "O tempo que a mensagem permanecerá invisível para outras filas.", Required = true)]
        public int InvisibilityTime { get; set; }

        public override IMessagingConfig GetConfiguration()
        {
            ValidateOption(nameof(ConnectionString), ConnectionString);
            ValidateOption(nameof(InvisibilityTime), InvisibilityTime);

            return new MessagingConfigBuilder()
                .WithAzureQueueBroker("azure", ConnectionString, InvisibilityTime, setAsDefault: true)
                .Create();
        }
    }
}
