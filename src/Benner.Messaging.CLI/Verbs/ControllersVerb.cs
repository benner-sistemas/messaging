using CommandLine;
using System.Collections.Generic;
using Benner.Messaging.Configuration;
using System.Linq;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("controllers", HelpText = "Configura os Controllers do 'producer.json'")]
    public class ControllersVerb : IVerb
    {
        [Option('c', "controllers", Required = true, Separator = ',', Min = 1,
            HelpText = "Configura o(s) controllers informados no 'producer.json'. Em caso de múltiplos controllers, separe-as por vírgula (,).")]
        public IEnumerable<string> Controllers { get; set; }

        public void Configure()
        {
            var producerJson = JsonConfiguration.LoadConfiguration<ProducerJson>() ?? new ProducerJson();

            var controllers = this.Controllers.ToList();
            if (controllers.Count > 0)
                producerJson.Controllers = controllers.Count == 1 && string.IsNullOrWhiteSpace(controllers[0]) 
                    ? new List<string>() 
                    : controllers.Select(c => c.Trim()).ToList();

            producerJson.SaveConfigurationToFile();
        }

        public bool HasNoInformedParams()
        {
            return !Controllers.Any();
        }

    }
}
