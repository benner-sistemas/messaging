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
            try
            {
                cliConfig.Execute();
                if (!string.IsNullOrWhiteSpace(cliConfig.Consumer) && cliConfig.Configuration != null)
                {
                    var consumer = ConsumerUtil.CreateConsumerByName(cliConfig.Consumer);
                    if (consumer == null)
                    {
                        Console.WriteLine($"Não foi encontrado uma classe '{cliConfig.Consumer}' em nenhum assembly.\r\n");
                        return 1;
                    }
                    Console.WriteLine($"Classe '{cliConfig.Consumer}' encontrada. Criando um listener...\r\n");
                    var listener = new EnterpriseIntegrationListener(cliConfig.Configuration, consumer);
                    listener.Start();
                    return 0;
                }
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine("ERROR(S):\r\n " + e.Message);
            }
            return 1;
        }
    }
}
