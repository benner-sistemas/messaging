using Benner.Messaging.Common;

namespace Benner.Consumer.Core
{
    public class ConsumerJson : JsonConfiguration 
    {
        public override string FileName => "consumer.json";

        public string Consumer { get; set; }
    }
}
