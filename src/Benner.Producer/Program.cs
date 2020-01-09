using Benner.Messaging;
using Benner.Messaging.CLI;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Benner.Producer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            bool succeededCliConfig = false;
            if (args.Length >= 1)
            {
                var parser = CliParserFactory.CreateForProducer(args);
                try
                {
                    parser.Parse();
                    if (!parser.HasValidationError && parser.Exception != null)
                    {
                        Console.WriteLine("ERROR(S):\r\n " + parser.Exception);
                        return;
                    }
                    if (parser.HasValidationError)
                    {
                        Console.WriteLine("ERROR(S):\r\n " + parser.Exception.Message);
                        return;
                    }
                    if (parser.HasParseError)
                        return;
                    if (parser.Configuration != null)
                        succeededCliConfig = true;
                }
                catch (Exception e)
                {
                    PrintErrorMessageWithTip(e.Message);
                    return;
                }
            }
            var configFileExists = FileMessagingConfig.DefaultConfigFileExists;
            if (succeededCliConfig && configFileExists)
            {
                PrintErrorMessageWithTip("Foi detectado um arquivo de configuração. " +
                    "Não é possível utilizar configuração por arquivo e por linha de comando simultaneamente.");
                return;
            }

            if (!succeededCliConfig && !configFileExists)
            {
                PrintErrorMessageWithTip("Não foi detectado um arquivo de configuração.");
                return;
            }
            CreateWebHostBuilder(args).Build().Run();
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
