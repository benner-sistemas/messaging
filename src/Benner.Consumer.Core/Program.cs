using Benner.Listener;
using Benner.Messaging.CLI;
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
                if (!cliConfig.HasValidationError && cliConfig.Exception != null)
                {
                    Console.WriteLine("ERROR(S):\r\n " + cliConfig.Exception);
                    return 1;
                }
                if (cliConfig.HasValidationError)
                {
                    Console.WriteLine("ERROR(S):\r\n " + cliConfig.Exception.Message);
                    return 1;
                }
                if (!string.IsNullOrWhiteSpace(cliConfig.Consumer) && cliConfig.Configuration != null)
                {
                    var consumer = ConsumerFactory.CreateConsumer(cliConfig.Consumer);
                    if (consumer == null)
                    {
                        Console.WriteLine($"Não foi encontrado uma classe '{cliConfig.Consumer}' em nenhum assembly.\r\n");
                        return 1;
                    }
                    Console.WriteLine($"Classe '{cliConfig.Consumer}' encontrada. Criando um listener...\r\n");
                    var listener = new EnterpriseIntegrationListener(cliConfig.Configuration, consumer);
                    listener.Start();
                    Console.WriteLine($"Escutando fila '{consumer.Settings.QueueName}' em broker '{cliConfig.BrokerName}'...");
                    Console.WriteLine("Pressione qualquer tecla para cancelar...");
                    Console.ReadKey();
                    Console.WriteLine("Ação cancelada pelo usuário.");
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR(S):\r\n " + e.Message);
            }
            return 1;
        }
    }
}
