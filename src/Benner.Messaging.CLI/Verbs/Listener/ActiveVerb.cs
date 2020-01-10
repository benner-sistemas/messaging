﻿using Benner.Messaging.Interfaces;
using CommandLine;

namespace Benner.Messaging.CLI.Verbs.Listener
{
    [Verb("active", HelpText = "Iniciar um listener para ActiveMQ")]
    public class ActiveVerb : ListenerVerb
    {
        [Option('h', "hostName", HelpText = "O nome do host.", Required = true)]
        public string Host { get; set; }

        [Option("port", HelpText = "A porta de comunicação com o broker.", Required = true)]
        public int Port { get; set; }

        [Option('u', "user", HelpText = "O usuário de login do broker.", Required = true)]
        public string User { get; set; }

        [Option('p', "password", HelpText = "A senha de login do broker.", Required = true)]
        public string Password { get; set; }

        public override string BrokerName => "ActiveMQ";

        public override IMessagingConfig GetConfiguration()
        {
            OptionValidator.ValidateOption("-n/--consumerName", Consumer);
            var producer = new Producer.ActiveVerb()
            {
                Host = this.Host,
                Password = this.Password,
                Port = this.Port,
                User = this.User
            };
            producer.ValidateParameters();
            return producer.GetConfiguration();
        }
    }
}