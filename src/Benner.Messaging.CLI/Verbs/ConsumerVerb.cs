using System;
using System.Collections.Generic;
using System.Text;
using Benner.Messaging.Configuration;
using CommandLine;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("consumer", HelpText = "Configura o arquivo 'consumer.json'")]
    public class ConsumerVerb : IVerb
    {
        [Option('c', "consumer", HelpText = "O nome completo da classe a ser procurada para ser usada como consumidor.", Required = true)]
        public string Consumer { get; set; }

        public void Configure()
        {
            var consumerJson = JsonConfiguration.LoadConfiguration<ConsumerJson>() ?? new ConsumerJson();
            consumerJson.Consumer = this.Consumer;
            consumerJson.SaveConfigurationToFile();
        }

        public bool HasNoInformedParams()
        {
            return Consumer == null;
        }
    }
}
