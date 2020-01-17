using Benner.Messaging.Common;
using System.Collections.Generic;

namespace Benner.Producer.Configuration
{
    internal class ProducerJson : JsonConfiguration
    {
        public override string FileName => "producer.json";

        public List<string> Controllers { get; set; }

        public OidcSettings Oidc { get; set; }

        public void EnsureExtensionOnControllers()
        {
            for (int i = 0; i < Controllers.Count; i++)
                if (!Controllers[i].EndsWith(".dll"))
                    Controllers[i] += ".dll";
        }
    }
}
