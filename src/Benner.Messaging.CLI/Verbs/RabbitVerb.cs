using CommandLine;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("rabbit", HelpText = "Configura um broker RabbitMQ no arquivo 'messaging.config'")]
    public class RabbitVerb : BrokerBase, IVerb
    {
        private const string RABBITMQ_TYPE = "Benner.Messaging.RabbitMQConfig, Benner.Messaging, Culture=neutral, PublicKeyToken=257abf4668fbf313";

        [Option('h', "hostName", HelpText = "O nome do host.")]
        public string Host { get; set; }

        [Option("port", HelpText = "A porta de comunicação com o broker.")]
        public int? Port { get; set; }

        [Option('u', "user", HelpText = "O usuário de login do broker.")]
        public string User { get; set; }

        [Option('p', "password", HelpText = "A senha de login do broker.")]
        public string Password { get; set; }

        public void Configure() => base.BaseConfigure(RABBITMQ_TYPE, GetRabbitAdds());

        public bool HasNoInformedParams()
        {
            return Host == null && Port == null && User == null && Password == null;
        }

        private XElement[] GetRabbitAdds()
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
