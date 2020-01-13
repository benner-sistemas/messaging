using Benner.Messaging;
using Benner.Producer.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Benner.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                BrokerConfiguration brokerConfiguration = new BrokerConfiguration(Utils.RemoveController(args));

                brokerConfiguration.SetConfiguration();

                CreateWebHostBuilder(args).Build().Run();
            }
            catch (Exception e)
            {
                if (string.IsNullOrWhiteSpace(e.Message))
                {
                    return;
                }

                PrintErrorMessageWithTip(e.Message);

                return;
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        private static void PrintErrorMessageWithTip(string msg)
        {
            Console.WriteLine("ERROR(S):");
            Console.WriteLine($"  {msg}");
            Console.WriteLine("Dica: utilize '--help'.");
        }
    }
}
