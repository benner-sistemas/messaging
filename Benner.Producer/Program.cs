using Benner.Producer.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Linq;

namespace Benner.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                BrokerConfiguration brokerConfiguration = CreateBrokerConfiguration(args);

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

        /// <summary>
        /// The broker configuration does not suport the argument --controller.
        /// </summary>
        private static BrokerConfiguration CreateBrokerConfiguration(string[] args)
        {
            if (args.Length == 1 &&
                args.Any(s => s.Contains("--controller")))
            {
                return new BrokerConfiguration(null);
            }
            else if (args.Length > 1 &&
                     args.Any(s => s.Contains("--controller")))
            {
                string[] argsWithoutController;

                if (args.Any(s => s.Contains("--controller=")))
                {
                    argsWithoutController = args.Where(w => !w.Contains("--controller=")).ToArray();
                }
                else
                {
                    int i = Array.IndexOf(args, "--controller");

                    argsWithoutController = args.Where(w => w != args[i] && w != args[i + 1]).ToArray();
                }

                return new BrokerConfiguration(argsWithoutController);
            }
            else
            {
                return new BrokerConfiguration(args);

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
