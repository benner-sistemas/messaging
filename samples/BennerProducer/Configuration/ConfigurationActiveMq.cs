using Benner.Messaging;
using System;

namespace BennerProducer.Configuration
{
    public class ConfigurationActiveMq : IConfiguration
    {
        private string host;
        private string user;
        private string password;
        private int? port;

        public ConfigurationActiveMq(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("host") ||
                    arg.ToLower().Contains("server"))
                {
                    host = arg.Split(':')[1];
                }
                if (arg.ToLower().Contains("user"))
                {
                    user = arg.Split(':')[1];
                }
                if (arg.ToLower().Contains("password"))
                {
                    password = arg.Split(':')[1];
                }
                if (arg.ToLower().Contains("port"))
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
                       .WithActiveMQBroker(brokerName: "ActiveMQ",
                                           hostName: host,
                                           port: port ?? 61616,
                                           userName: user ?? "admin",
                                           password: password ?? "admin",
                                           setAsDefault: true)
                       .Create();
        }
    }
}
