using Benner.Listener;
using Benner.Messaging;
using Benner.Messaging.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

namespace Benner.Consumer.Core
{
    public class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var consumerConfig = JsonConfiguration.LoadConfiguration<ConsumerJson>();

                if (consumerConfig == null || string.IsNullOrWhiteSpace(consumerConfig?.Consumer))
                    throw new FileNotFoundException($"Não foi encontrado o arquivo '{new ConsumerJson().FileName}' no diretório atual.");

                var consumerClass = consumerConfig.Consumer;
                var consumer = GetConsumerByClassName(consumerClass)
                    ?? throw new FileNotFoundException($"Não foi encontrado uma classe '{consumerClass}' em nenhum assembly.");

                var brokersConfig = new FileMessagingConfig();
                Console.WriteLine($"Classe '{consumerClass}' encontrada. Criando um listener...\r\n");

                var listener = new EnterpriseIntegrationListener(brokersConfig, consumer);
                listener.Start();
                string queueName = consumer.Settings.QueueName;
                Console.WriteLine($"Escutando fila '{queueName}' em broker '{brokersConfig.GetBrokerNameForQueue(queueName)}'...\r\n");
                Thread.Sleep(Timeout.Infinite);
                Console.WriteLine("Ação cancelada pelo usuário.");
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERRO:\r\n " + e.Message);
                var comparer = StringComparer.OrdinalIgnoreCase;
                if (args.Contains("-verbose", comparer) || args.Contains("-v", comparer) || args.Contains("--verbose", comparer))
                    Console.WriteLine(e);
            }
            return 1;
        }

        /// <summary>
        /// Cria uma instancia de <see cref="IEnterpriseIntegrationConsumer"/> de acordo com o nome completo da classe.
        /// Retorna null caso não seja encontrada uma classe.
        /// </summary>
        /// <param name="fullName">O nome da classe completo. Ex: "Namespace.Namespace.NomeClasse".</param>
        /// <example>
        /// Nome completo de classe = Entidades.Pessoa
        /// Nome estilo assembly qualified = Entidades.Pessoa, Entidades
        /// </example>
        public static IEnterpriseIntegrationConsumer GetConsumerByClassName(string fullName)
        {
            if (fullName == null)
                return null;

            string folder = Directory.GetCurrentDirectory();
            string[] assembliesPathes = Directory.EnumerateFiles(folder, "*.Consumer.dll", SearchOption.TopDirectoryOnly).ToArray();

            if (assembliesPathes.Length == 0)
                throw new FileNotFoundException("Não foi encontrado qualquer assembly com pattern '*.Consumer.dll' no diretório de trabalho.");

            foreach (string assemblyPath in assembliesPathes)
            {
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);

                Type found = assembly.GetType(fullName, false, true);
                if (found == null)
                    found = assembly.GetExportedTypes().FirstOrDefault(x => x.AssemblyQualifiedName.StartsWith(fullName, StringComparison.OrdinalIgnoreCase));

                if (found != null)
                {
                    var isConsumer = typeof(IEnterpriseIntegrationConsumer).IsAssignableFrom(found);
                    var hasDefaultCtor = found.GetConstructor(Type.EmptyTypes) != null;
                    var isPubClass = found.IsClass && found.IsPublic;
                    if (isConsumer && hasDefaultCtor && isPubClass)
                    {
                        LoadReferencedAssembly(assembly);
                        return Activator.CreateInstance(found) as IEnterpriseIntegrationConsumer;
                    }
                }
            }

            throw new FileNotFoundException($"Não foi encontrado a classe '{fullName}' em todos os assemblies Consumer encontrados.");
        }

        private static void LoadReferencedAssembly(Assembly assembly)
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                {
                    try
                    {
                        LoadReferencedAssembly(Assembly.Load(name));
                    }
                    catch (FileNotFoundException)
                    {
                        LoadReferencedAssembly(
                            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Directory.GetCurrentDirectory(), $"{name.Name}.dll")));
                    }
                }
            }
        }
    }
}
