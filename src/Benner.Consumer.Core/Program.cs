using Benner.Listener;
using Benner.Messaging.CLI;
using System;

namespace Benner.Consumer.Core
{
    public class Program
    {
        static int Main(string[] args)
        {
            var parser = CliParserFactory.CreateForListener(args);
            try
            {
                parser.Parse();
                if (!parser.HasValidationError && parser.Exception != null)
                {
                    Console.WriteLine("ERROR(S):\r\n " + parser.Exception);
                    return 1;
                }
                if (parser.HasValidationError)
                {
                    Console.WriteLine("ERROR(S):\r\n " + parser.Exception.Message);
                    return 1;
                }
                if (!string.IsNullOrWhiteSpace(parser.Consumer) && parser.Configuration != null)
                {
                    var consumer = ConsumerFactory.CreateConsumer(parser.Consumer);
                    if (consumer == null)
                    {
                        Console.WriteLine($"Não foi encontrado uma classe '{parser.Consumer}' em nenhum assembly.\r\n");
                        return 1;
                    }
                    Console.WriteLine($"Classe '{parser.Consumer}' encontrada. Criando um listener...\r\n");
                    var listener = new EnterpriseIntegrationListener(parser.Configuration, consumer);
                    listener.Start();
                    Console.WriteLine($"Escutando fila '{consumer.Settings.QueueName}' em broker '{parser.BrokerName}'...");
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
