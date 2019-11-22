using Benner.Messaging;

namespace Benner.Messaging.Configuration
{
    public class ConfigurationAzureQueue : IConfiguration
    {
        private int invisibilityTime;
        private string connectionString;

        public ConfigurationAzureQueue(string[] args)
        {
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("-invisibilitytime:"))
                {
                    var parameter = arg.Split(':')[1];

                    int.TryParse(parameter, out invisibilityTime);
                }
                if (arg.ToLower().Contains("-connectionstring:"))
                {
                    connectionString = arg.Split(':')[1];
                }
            }
        }

        public string Validation()
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return "ConnectionString not defined";
            }

            return null;
        }

        public MessagingConfig Configuration()
        {

            return new MessagingConfigBuilder()
                       .WithAzureQueueBroker(brokerName: "AzureQueue",
                                             connectionString: connectionString,
                                             invisibilityTime: invisibilityTime,
                                             setAsDefault: true)
                       .Create();
        }
    }
}
