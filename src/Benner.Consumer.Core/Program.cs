using Benner.Listener;
using Benner.Messaging;
using Benner.Messaging.Common;
using Benner.Messaging.Logger;
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
                Log.ConfigureLog();
                var consumerConfig = JsonConfiguration.LoadConfiguration<ConsumerJson>();

                if (consumerConfig == null)
                    throw new FileNotFoundException($"Não foi encontrado o arquivo '{new ConsumerJson().FileName}' no diretório atual.");

                if (string.IsNullOrWhiteSpace(consumerConfig.Consumer))
                    throw new ArgumentException($"A propriedade 'Consumer' do arquivo '{new ConsumerJson().FileName}' não pode ser vazia.");

                var consumerClass = consumerConfig.Consumer;
                var consumer = GetConsumerByClassName(consumerClass);

                var brokersConfig = new FileMessagingConfig();
                Log.Information("Classe {consumerClass} encontrada. Criando um listener...", consumerClass);

                var listener = new EnterpriseIntegrationListener(brokersConfig, consumer);
                string queueName = consumer.Settings.QueueName;
                Log.Information("Iniciando a escuta da fila {queueName} do broker {brokerName}", queueName, brokersConfig.GetBrokerNameForQueue(queueName));
                listener.Start();
                Thread.Sleep(Timeout.Infinite);
                Log.Information("Listener encerrado por ação do usuário.");
                return 0;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }
            return 1;
        }

        /// <summary>
        /// Cria uma instancia de <see cref="IEnterpriseIntegrationConsumer"/> de acordo com o nome completo da classe.
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
                throw new FileNotFoundException("Não foi encontrado qualquer assembly com pattern '*.Consumer.dll' no diretório de trabalho atual.");

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

            throw new FileNotFoundException($"Não foi possível encontrar a classe '{fullName}' no diretório de trabalho atual.");
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
