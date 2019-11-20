using Benner.Messaging;

namespace BennerProducer.Configuration
{
    public class ConfigurationAmazonSqs : IConfiguration
    {
        private int invisibilityTime;

        public ConfigurationAmazonSqs(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("invisibilitytime"))
                {
                    var parameter = arg.Split(':')[1];

                    int.TryParse(parameter, out invisibilityTime);
                }
            }
        }

        public string Validation()
        {
            return null;
        }

        public MessagingConfig Configuration()
        {
            return new MessagingConfigBuilder()
                       .WithAmazonSQSBroker(brokerName: "AmazonSQS",
                                            invisibilityTime: invisibilityTime,
                                            setAsDefault: true)
                       .Create();
        }
    }
}
