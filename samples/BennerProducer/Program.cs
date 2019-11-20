using Benner.Messaging;
using BennerProducer.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace BennerProducer
{
    public class Program
    {
        public static MessagingConfig BrokerConnection
        {
            get;
            private set;
        }

        public static void Main(string[] args)
        {
            IConfiguration brokerConfiguration = new ConfigurationFactory().GetConfiguration(args);
            brokerConfiguration.Validation();
            BrokerConnection = brokerConfiguration.Configuration();

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
