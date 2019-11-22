using Benner.Messaging;
using System;

namespace Benner.Messaging.Configuration
{
    public class ConfigurationRabbitMq : IConfiguration
    {
        private string host;
        private string user;
        private string password;
        private int? port;

        public ConfigurationRabbitMq(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("-host:") ||
                    arg.ToLower().Contains("-server:"))
                {
                    host = arg.Split(':')[1];
                }
                if (arg.ToLower().Contains("-user:"))
                {
                    user = arg.Split(':')[1];
                }
                if (arg.ToLower().Contains("-password:"))
                {
                    password = arg.Split(':')[1];
                }
                if (arg.ToLower().Contains("-port:"))
                {
                    port = Convert.ToInt32(arg.Split(':')[1]);
                }
            }
        }

        public string Validation()
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return "Host name not defined";
            }
            if (port.HasValue &&
                port.Value < 0)
            {
                return "Port invalid";
            }

            return null;
        }

        public MessagingConfig Configuration()
        {
            return new MessagingConfigBuilder()
                        .WithRabbitMQBroker(brokerName: "RabbitMQ",
                                            hostName: host,
                                            port: port ?? 5672,
                                            userName: user ?? "guest",
                                            password: password ?? "guest",
                                            setAsDefault: true)
                        .Create();
        }
    }
}
