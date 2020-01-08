﻿using Benner.Producer.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
        ///  - Using command line argument, if no config file exists or if config file allows the override via command line.
        ///  - Using config file, if the file exits and doesn't load any assembly via command line.
        ///  - Loading all assemblies "*.Producer.dll" from the current directory if doesn't load any assembly.
        /// </summary>
        /// <param name="cliControllers">Controller's assemblies enter via command line.</param>
        /// <exception cref="Exception">If no assemblies found.</exception>
        public void SetAssemblyControllers(string cliControllers)
        {
            var producerFileConfig = GetConfigurationFile();

            bool existsCommandLineDefinition = !string.IsNullOrWhiteSpace(cliControllers);
            bool existsConfigFile = producerFileConfig != null && producerFileConfig.Controllers != null && producerFileConfig.Controllers.Count > 0;
            bool useCommandLine = (!existsConfigFile || producerFileConfig.UseCommandLine) && existsCommandLineDefinition;

            AssembliesControllers = new List<Assembly>();

            if (useCommandLine)
            {
                var listAssembliesControllers = cliControllers.Split(',');

                foreach (var assembly in listAssembliesControllers)
                {
                    AssembliesControllers.Add(Assembly.Load(assembly));
                }
            }

            if (existsConfigFile &&
                AssembliesControllers.Count < 1)
            {
                foreach (var controller in producerFileConfig.Controllers)
                {
                    AssembliesControllers.Add(Assembly.Load(controller.assembly));
                }
            }

            if (AssembliesControllers.Count < 1)
            {
                string[] assembliesPathes = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), "*.Producer.dll", SearchOption.TopDirectoryOnly).ToArray();

                if (assembliesPathes.Length == 0)
                {
                    throw new Exception("Não foram encontrados assemblies de Controllers no diretório.");
                }

                Console.WriteLine("Foram carregados os Controllers de todos os assemblies \"*.Producer.dll\" presentes do diretório atual.");

                foreach (var assembly in assembliesPathes)
                {
                    AssembliesControllers.Add(Assembly.Load(assembly));
                }
            }
        }

        /// <summary>
        /// Get the configuration file (producer.json), if it exists, and convert into a ProducerJson object.
        /// </summary>
        /// <returns>The configuration file "producer.json" converted to ProducerJson. If the file doesn't exists, return null.</returns>
        /// <remarks>If the file doesn't exists or exists but without controllers configuration, return null.</remarks>
        private ProducerJson GetConfigurationFile()
        {
            string fileConfigPath = Path.Combine(Directory.GetCurrentDirectory(), "producer.json");

            if (File.Exists(fileConfigPath))
            {
                var configJson = File.ReadAllText(fileConfigPath);

                if (!string.IsNullOrWhiteSpace(configJson))
                {
                    return JsonConvert.DeserializeObject<ProducerJson>(configJson);
                }
            }

            return null;
        }
    }
}