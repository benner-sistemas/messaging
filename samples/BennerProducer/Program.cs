using Benner.Messaging.Common;
using Benner.Messaging.Interfaces;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace BennerProducer
{
    public class Program
    {
        public static IMessagingConfig BrokerConnection { get; private set; }

        public static void Main(string[] args)
        {
            //BrokerConnection = BrokerConfiguration.SetConfiguration(args);
            var cliConfig = new CliConfiguration(args);
            cliConfig.Execute();
            BrokerConnection = cliConfig.Configuration;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
