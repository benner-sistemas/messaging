using Benner.Listener;
using Benner.Messaging.Common;
using System;

namespace Benner.Consumer.Core
{
    public class Program
    {
        static int Main(string[] args)
        {
            var cliConfig = new CliConfiguration(args);
            if (cliConfig.Exception != null)
            {
                Console.WriteLine(cliConfig.Exception.Message);
                return 1;
            }
            if (!string.IsNullOrWhiteSpace(cliConfig.Consumer) && cliConfig.Configuration != null)
            {
                var consumer = ConsumerUtil.CreateConsumerByName(cliConfig.Consumer);
                if (consumer == null)
                {
                    Console.WriteLine($"Não foi encontrado uma classe '{cliConfig.Consumer}' em nenhum assembly.\n");
                    return 1;
                }
                Console.WriteLine($"Classe '{cliConfig.Consumer}' encontrada. Criando um listener...\n");
                var listener = new EnterpriseIntegrationListener(cliConfig.Configuration, consumer);
                listener.Start();
            }

            return Convert.ToInt32(cliConfig.IsError);
        }
    }
}
