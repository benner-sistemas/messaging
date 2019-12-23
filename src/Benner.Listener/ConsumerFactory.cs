using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Benner.Listener
{
    /// <summary>
    /// Classe responsável por criar uma instância de <see cref="IEnterpriseIntegrationConsumer"/>
    /// </summary>
    public static class ConsumerFactory
    {
        private static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            return new FileInfo(location.LocalPath).Directory.FullName;
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
        public static IEnterpriseIntegrationConsumer CreateConsumer(string fullName)
        {
            if (fullName == null)
                return null;

            string folder = GetExecutingDirectoryName();
            string[] assembliesPathes = Directory.EnumerateFiles(folder, "*.Consumer.Core.dll", SearchOption.TopDirectoryOnly).ToArray();

            if (assembliesPathes.Length == 0)
                throw new FileNotFoundException("Não foi encontrado qualquer assembly com pattern '*.Consumer.dll' no diretório de trabalho.");

            foreach (string assemblyPath in assembliesPathes)
            {
                Assembly assembly = Assembly.Load(File.ReadAllBytes(assemblyPath));
                Type found = assembly.GetType(fullName, false, true);
                if (found == null)
                    found = assembly.GetExportedTypes().FirstOrDefault(x => x.AssemblyQualifiedName.StartsWith(fullName, StringComparison.OrdinalIgnoreCase));

                if (found != null)
                {
                    var isConsumer = typeof(IEnterpriseIntegrationConsumer).IsAssignableFrom(found);
                    var hasDefaultCtor = found.GetConstructor(Type.EmptyTypes) != null;
                    var isPubClass = found.IsClass && found.IsPublic;
                    if (isConsumer && hasDefaultCtor && isPubClass)
                        return Activator.CreateInstance(found) as IEnterpriseIntegrationConsumer;
                }
            }
            throw new FileNotFoundException($"Não foi encontrado a classe '{fullName}' em todos os assemblies Consumer encontrados.");
        }
    }
}
