using Benner.Messaging.Interfaces;
using CommandLine;

namespace Benner.Messaging.Common.Verbs
{
    [Verb("amazon", HelpText = "Iniciar um listener para Amazon SQS")]
    public class AmazonVerb : ListenVerb
    {
        [Option('i', "invisibilityTime", HelpText = "O tempo que a mensagem permanecerá invisível para outras filas.", Required = true)]
        public int InvisibilityTime { get; set; }

        public override IMessagingConfig GetConfiguration()
        {
            ValidateOption(nameof(InvisibilityTime), InvisibilityTime);

            return new MessagingConfigBuilder()
                .WithAmazonSQSBroker("amazon", invisibilityTime: InvisibilityTime, setAsDefault: true)
                .Create();
        }
    }
}
