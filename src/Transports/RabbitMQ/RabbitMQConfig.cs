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

        public RabbitMQConfig(Dictionary<string, string> brokerSettings)
        {
            HostName = brokerSettings.GetValue("HostName", true);
            UserName = brokerSettings.GetValue("UserName", false, "guest");
            Password = brokerSettings.GetValue("Password", false, "guest");

            string informedPort = brokerSettings.GetValue("Port", false);
            if (!string.IsNullOrWhiteSpace(informedPort))
            {
                if (!int.TryParse(informedPort, out int port))
                    throw new ArgumentException("Informed port is not valid.");
                Port = port;
            }
            else
                Port = 5672;
        }

        public BrokerTransport CreateTransporterInstance() => new RabbitMQTransport(this);
    }
}