using Benner.Messaging.Configuration;
using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Benner.Messaging.CLI.Verbs
{
    [Verb("logging", HelpText = "Configura o arquivo 'logSettings.json'")]
    public class LogsSettingsVerb : IVerb
    {
        [Option('n', "nodeUris", Separator = ',', Min = 1,
            HelpText = "Configura o(s) endereço(s) do(s) servidor(es) ElasticSearch. Em caso de múltiplos endereços, separe-os por vírgula (,).")]
        public IEnumerable<string> NodeUris { get; set; }

        [Option('e', "enableElasticSearch", HelpText = "Habilita o envio de logs para um servidor ElasticSearch. Valores válidos: true, false.")]
        public bool? EnableElasticSearch { get; set; }

        [Option('m', "minimumLogLevel", HelpText = "Informa qual o nível mínimo de log.")]
        public ILogLevels? MinimumLogLevel { get; set; }

        public void Configure()
        {
            var logJson = JsonConfiguration.LoadConfiguration<LogJson>() ?? new LogJson();

            if (logJson.ElasticSearch == null)
                logJson.ElasticSearch = new ElasticSearchSettings();

            if (this.EnableElasticSearch.HasValue)
                logJson.EnableElasticSearch = this.EnableElasticSearch;

            var uris = this.NodeUris.ToList();
            if (uris.Count > 0)
                logJson.ElasticSearch.NodeUris = uris.Count == 1 && string.IsNullOrWhiteSpace(uris[0])
                    ? new List<string>()
                    : uris.Select(c => c.Trim()).ToList();

            if (this.MinimumLogLevel != null)
                logJson.MinimumLogLevel = Enum.GetName(typeof(ILogLevels), this.MinimumLogLevel);

            logJson.SaveConfigurationToFile();
        }

        public bool HasNoInformedParams()
        {
            return !NodeUris.Any() && EnableElasticSearch == null && MinimumLogLevel == null;
        }
    }

    public enum ILogLevels
    {
        Verbose = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5
    }
}
