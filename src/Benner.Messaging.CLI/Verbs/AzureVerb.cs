using CommandLine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("azure", HelpText = "Configura um broker Azure Queue no arquivo 'messaging.config'")]
    public class AzureVerb : BrokerBase, IVerb
    {
        private const string AZURE_TYPE = "Benner.Messaging.AzureQueueConfig, Benner.Messaging, Culture=neutral, PublicKeyToken=257abf4668fbf313";

        [Option('c', "connectionString", HelpText = "A string de conexão com o serviço Azure.")]
        public string ConnectionString { get; set; }

        [Option('i', "invisibilityTime", HelpText = "O tempo que a mensagem permanecerá invisível para outras filas, em segundos.")]
        public int? InvisibilityTime { get; set; }

        public void Configure() => base.BaseConfigure(AZURE_TYPE, GetAzureAdds());

        public bool HasNoInformedParams()
        {
            return InvisibilityTime == null && ConnectionString == null;
        }

        private XElement[] GetAzureAdds()
        {
            var adds = new List<XElement>();

            if (!string.IsNullOrWhiteSpace(ConnectionString))
                adds.Add(CreateNodeAdd("ConnectionString", ConnectionString));

            if (InvisibilityTime != null)
                adds.Add(CreateNodeAdd("Port", InvisibilityTime.ToString()));

            return adds.ToArray();
        }
    }
}
