using System;
using System.Collections.Generic;
using Benner.Messaging.Interfaces;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração do RabbitMQ.
    /// </summary>
    internal class RabbitMQConfig : IInternalBrokerConfig
    {
        internal string HostName { get; set; }
        internal string UserName { get; set; }
        internal string Password { get; set; }
        internal int Port { get; set; }

        public RabbitMQConfig(Dictionary<string, string> configurations)
        {
            HostName = configurations.GetValue("HostName", true);
            UserName = configurations.GetValue("UserName", false, "guest");
            Password = configurations.GetValue("Password", false, "guest");

            string informedPort = configurations.GetValue("Port", false);
            if (!string.IsNullOrWhiteSpace(informedPort))
            {
                if (!int.TryParse(informedPort, out int port))
                    throw new ArgumentException("Porta informada não é válida");
                Port = port;
            }
            else
                Port = 5672;
        }

        public BrokerTransport CreateTransporterInstance() => new RabbitMQTransport(this);
    }
}