using System.Collections.Generic;

namespace Benner.Producer.Models
{
    public class ProducerJson
    {
        public bool UseCommandLine { get; set; }

        public List<Controller> Controllers { get; set; }
    }

    public class Controller
    {
        public string assembly { get; set; }
    }
}
