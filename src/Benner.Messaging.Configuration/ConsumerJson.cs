
namespace Benner.Messaging.Configuration
{
    public class ConsumerJson : JsonConfiguration 
    {
        public override string FileName => "consumer.json";

        public string Consumer { get; set; }
    }
}
