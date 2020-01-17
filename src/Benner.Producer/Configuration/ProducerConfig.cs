using System.Collections.Generic;
using System.Linq;

namespace Benner.Producer.Configuration
{
    internal class ProducerConfig : Configuration
    {
        protected override string FileName => "producer.json";

        public List<string> Controllers { get; set; }

        public void EnsureExtensionOnControllers()
        {
            for (int i = 0; i < Controllers.Count; i++)
                if (!Controllers[i].EndsWith(".dll"))
                    Controllers[i] += ".dll";
        }
    }
}
