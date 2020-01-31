using System;
using System.Collections.Generic;
using System.Linq;

namespace Benner.Messaging.Configuration
{
    public class LogJson : JsonConfiguration
    {
        public override string FileName => "logSettings.json";

        public ElasticSearchSettings ElasticSearch { get; set; }

        public string MinimumLogLevel { get; set; }

        public bool EnableElasticSearch { get; set; }
    }

    public class ElasticSearchSettings
    {
        public List<string> NodeUris { get; set; }

        public IEnumerable<Uri> GetUris() => NodeUris.Select(x => new Uri(x));
    }
}
