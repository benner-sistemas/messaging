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
            OptionValidator.ValidateOption("-n/--consumerName", Consumer);
            var producer = new Producer.AmazonVerb()
            {
                AccessKeyId = this.AccessKeyId,
                InvisibilityTime = this.InvisibilityTime,
                SecretAccessKey = this.SecretAccessKey
            };
            producer.ValidateParameters();
            return producer.GetConfiguration();
        }
    }
}
