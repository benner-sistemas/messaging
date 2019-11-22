namespace Benner.Messaging.Configuration
{
    public class BrokerConfiguration
    {
        public static MessagingConfig SetConfiguration(string[] args)
        {
            IConfiguration brokerConfiguration = new ConfigurationFactory().GetConfiguration(args);
            brokerConfiguration.Validation();
            return brokerConfiguration.Configuration();
        }
    }
}
