using Benner.Messaging.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Benner.Consumer.Core
{
    internal class ConsumerUtil
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
        /// <returns></returns>
        internal static IEnterpriseIntegrationConsumer CreateConsumerByName(string fullName)
        {
            if (fullName == null)
                return null;

            string folder = GetExecutingDirectoryName();
            IEnumerable<string> assembliesPathes = Directory.EnumerateFiles(folder, "*.dll", SearchOption.TopDirectoryOnly)
                .Where(a =>
                {
                    var fileName = Path.GetFileName(a);
                    return !fileName.StartsWith("System") && !fileName.StartsWith("Microsoft");
                });

            foreach (string assemblyPath in assembliesPathes)
            {
                AppDomain tempDomain = AppDomain.CreateDomain("Temp Domain");
                try
                {
                    Assembly assembly = tempDomain.Load(File.ReadAllBytes(assemblyPath));
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
                finally
                {
                    AppDomain.Unload(tempDomain);
                }
            }
            return null;
        }
    }
}
