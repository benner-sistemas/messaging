using Benner.Messaging.Interfaces;
using CommandLine;
using System;
namespace Benner.Messaging.CLI.Verbs.Listener
{
    [Verb("amazon", HelpText = "Iniciar um listener para Amazon SQS")]
    public class AmazonVerb : ListenerVerb
    {
        [Option('i', "invisibilityTime", HelpText = "O tempo que a mensagem permanecerá invisível para outras filas, em segundos.", Required = true)]
        public int InvisibilityTime { get; set; }

        [Option('a', "accessKeyId", HelpText = "O 'Key ID' de acesso ao servidor Amazon. Se informado, 'secretAccessKey' também deve ser informado.", Required = false)]
        public string AccessKeyId { get; set; }

        [Option('s', "secretAccessKey", HelpText = "O 'Secret Access Key' de acesso ao servidor Amazon. Se informado, 'accessKeyId' também deve ser informado.", Required = false)]
        public string SecretAccessKey { get; set; }

        public override string BrokerName => "AmazonSQS";

        public override IMessagingConfig GetConfiguration()
        {
            ValidateParameters();

            if (string.IsNullOrWhiteSpace(AccessKeyId) && string.IsNullOrWhiteSpace(SecretAccessKey))
                return new MessagingConfigBuilder()
                .WithAmazonSQSBroker("amazon", InvisibilityTime, setAsDefault: true)
                .Create();

            return new MessagingConfigBuilder()
            .WithAmazonSQSBroker("amazon", InvisibilityTime, AccessKeyId, SecretAccessKey, setAsDefault: true)
            .Create();
        }

        public override void ValidateParameters()
        {
            OptionValidator.ValidateOption("-n/--consumerName", Consumer);
            OptionValidator.ValidateOption("-i/--invisibilityTime", InvisibilityTime);

            if (!string.IsNullOrWhiteSpace(AccessKeyId) != !string.IsNullOrWhiteSpace(SecretAccessKey))
                throw new ArgumentException($"O parâmetro 'accessKeyId' ou 'secretAccessKey' são obrigatórios caso um deles seja informado.");
        }
    }
}
