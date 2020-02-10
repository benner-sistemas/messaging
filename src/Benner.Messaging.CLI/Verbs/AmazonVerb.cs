using CommandLine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("amazon", HelpText = "Configura um broker Amazon SQS no arquivo 'messaging.config'")]
    public class AmazonVerb : BrokerBase, IVerb
    {
        private const string AMAZONSQS_TYPE = "Benner.Messaging.AmazonSQSConfig, Benner.Messaging, Culture=neutral, PublicKeyToken=257abf4668fbf313";

        [Option('i', "invisibilityTime", HelpText = "O tempo que a mensagem permanecerá invisível para outras filas, em segundos.")]
        public int? InvisibilityTime { get; set; }

        [Option('a', "accessKeyId", HelpText = "O 'Key ID' de acesso ao servidor Amazon. Se informado, 'secretAccessKey' também deve ser informado.")]
        public string AccessKeyId { get; set; }

        [Option('s', "secretAccessKey", HelpText = "O 'Secret Access Key' de acesso ao servidor Amazon. Se informado, 'accessKeyId' também deve ser informado.")]
        public string SecretAccessKey { get; set; }

        public void Configure() => base.BaseConfigure(AMAZONSQS_TYPE, GetAmazonAdds());

        public bool HasNoInformedParams()
        {
            return InvisibilityTime == null && AccessKeyId == null && SecretAccessKey == null;
        }

        private XElement[] GetAmazonAdds()
        {
            var adds = new List<XElement>();

            if (!string.IsNullOrWhiteSpace(AccessKeyId))
                adds.Add(CreateNodeAdd("AccessKeyId", AccessKeyId));

            if (!string.IsNullOrWhiteSpace(SecretAccessKey))
                adds.Add(CreateNodeAdd("SecretAccessKey", SecretAccessKey));

            if (InvisibilityTime != null)
                adds.Add(CreateNodeAdd("Port", InvisibilityTime.ToString()));

            return adds.ToArray();
        }
    }
}
