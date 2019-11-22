using Benner.Messaging;
using Benner.Messaging.Configuration;
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
            BrokerConnection = BrokerConfiguration.SetConfiguration(args);

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
