using Benner.Messaging.Common;
using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração do ActiveMQ.
    /// </summary>
    internal class ActiveMQConfig : IInternalBrokerConfig
    {
        internal string Hostname { get; set; }
        internal int Port { get; set; }
        internal string Username { get; set; }
        internal string Password { get; set; }

        public ActiveMQConfig(Dictionary<string, string> configurations)
        {
            Hostname = configurations.GetValue("Hostname", true);
            Username = configurations.GetValue("Username", false, "admin");
            Password = configurations.GetValue("Password", false, "admin");

            string informedPort = configurations.GetValue("Port", false);

            if (!string.IsNullOrWhiteSpace(informedPort))
            {
                if (!int.TryParse(informedPort, out int port))
                    throw new ArgumentException(string.Format(ErrorMessages.InvalidInformed, "port"));
                Port = port;
            }
            else
                Port = 61616;
        }

        public BrokerTransport CreateTransporterInstance() => new ActiveMQTransport(this);
    }
}
