using Benner.Messaging.CLI;
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
            var cliConfig = CliParserFactory.CreateForProducer(args);
            cliConfig.Parse();
            BrokerConnection = cliConfig.Configuration;
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
