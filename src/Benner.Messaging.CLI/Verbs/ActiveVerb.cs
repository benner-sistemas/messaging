using CommandLine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Benner.Messaging.CLI.Verbs
{

    [Verb("active", HelpText = "Configura um broker ActiveMQ no arquivo 'messaging.config'")]
    public class ActiveVerb : BrokerBase, IVerb
    {
        private const string ACTIVEMQ_TYPE = "Benner.Messaging.ActiveMQConfig, Benner.Messaging, Culture=neutral, PublicKeyToken=257abf4668fbf313";

        [Option('h', "hostName", HelpText = "O nome do host.")]
        public string Host { get; set; }

        [Option("port", HelpText = "A porta de comunicação com o broker.")]
        public int? Port { get; set; }

        [Option('u', "user", HelpText = "O usuário de login do broker.")]
        public string User { get; set; }

        [Option('p', "password", HelpText = "A senha de login do broker.")]
        public string Password { get; set; }

        public void Configure() => base.BaseConfigure(ACTIVEMQ_TYPE, GetActiveAdds());

        public bool HasNoInformedParams()
        {
            return Host == null && Port == null && User == null && Password == null;
        }

        private XElement[] GetActiveAdds()
        {
            var adds = new List<XElement>();

            if (!string.IsNullOrWhiteSpace(Host))
                adds.Add(CreateNodeAdd("Hostname", Host));

            if (Port != null)
                adds.Add(CreateNodeAdd("Port", Port.ToString()));

            if (!string.IsNullOrWhiteSpace(User))
                adds.Add(CreateNodeAdd("Username", User));

            if (!string.IsNullOrWhiteSpace(Password))
                adds.Add(CreateNodeAdd("Password", Password));

            return adds.ToArray();
        }
    }
}
