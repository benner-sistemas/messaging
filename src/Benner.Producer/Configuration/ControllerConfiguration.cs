using Benner.Messaging.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Benner.Producer.Configuration
{
    public class ControllerConfiguration
    {
        /// <summary>
        /// List of controller's assemblies 
        /// </summary>
        public List<Assembly> AssembliesControllers { get; private set; }

        /// <summary>
        /// Set the property AssembliesControllers:
        ///  - Using config file, if the file exits.
        ///  - Loading all assemblies "*.Producer.dll" from the current directory when no configuration file exists.
        /// </summary>
        /// <param name="cliControllers">Controller's assemblies enter via command line.</param>
        /// <exception cref="Exception">If no assemblies found.</exception>
        public void SetAssemblyControllers()
        {
            var producerFileConfig = JsonConfiguration.LoadConfiguration<ProducerJson>();
            if (producerFileConfig == null)
                throw new FileNotFoundException("O arquivo 'producer.json' não foi encontrado.");

            bool controllersIsEmpty = producerFileConfig.Controllers?.Count < 1;
            var path = Directory.GetCurrentDirectory();
            string[] assembliesPathes = Directory.EnumerateFiles(path, "*.Producer.dll", SearchOption.TopDirectoryOnly)
                .Where(a => !a.EndsWith("Benner.Producer.dll"))
                .ToArray();

            if (controllersIsEmpty && assembliesPathes.Length == 0)
                throw new FileLoadException($"O arquivo de configuração '{new ProducerJson().FileName}' não contém uma lista de controllers, " +
                    $"e nenhum assembly de sufixo '*.Producer.dll' foi encontrado no diretório de trabalho atual.");

            AssembliesControllers = new List<Assembly>();

            if (!controllersIsEmpty)
            {
                producerFileConfig.EnsureExtensionOnControllers();
                foreach (string controller in producerFileConfig.Controllers)
                    if (!string.IsNullOrWhiteSpace(controller))
                        LoadAssemblyReferencesAndAddToControllers(Path.Combine(path, controller));
            }
            else
            {
                foreach (var assembly in assembliesPathes)
                    LoadAssemblyReferencesAndAddToControllers(assembly);

                Console.WriteLine("Foram carregados os Controllers de todos os assemblies \"*.Producer.dll\" presentes do diretório atual.");
            }
        }

        private void LoadAssemblyReferencesAndAddToControllers(string assemblyPath)
        {
            if (!File.Exists(assemblyPath))
            {
                var fi = new FileInfo(assemblyPath);
                throw new FileNotFoundException($"O assembly '{fi.Name}' não foi encontrado no diretório '{fi.DirectoryName}'");
            }
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            AssembliesControllers.Add(assembly);
            LoadReferencedAssembly(assembly);
        }

        private void LoadReferencedAssembly(Assembly assembly)
        {
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                if (!AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == name.FullName))
                {
                    try
                    {
                        this.LoadReferencedAssembly(Assembly.Load(name));
                    }
                    catch (FileNotFoundException)
                    {
                        this.LoadReferencedAssembly(
                            AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Directory.GetCurrentDirectory(), $"{name.Name}.dll")));
                    }
                }
            }
        }
    }
}
