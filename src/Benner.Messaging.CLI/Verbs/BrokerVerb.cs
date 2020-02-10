using Benner.Messaging.Configuration;
using CommandLine;
using System;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace Benner.Messaging.CLI.Verbs
{
    public class BrokerBase
    {
        private string _filePath;
        public string FilePath
        {
            get
            {
                if (_filePath == null)
                    _filePath = Path.Combine(DirectoryHelper.GetExecutingDirectoryName(), "messaging.config");

                return _filePath;
            }
        }

        [Option('n', "brokerName", HelpText = "O nome de referência do broker.", Required = true)]
        public string BrokerName { get; set; }

        [Option("isDefault", HelpText = "Define se a configuração informada será marcada como default. Valores válidos: true, false.", Required = true)]
        public bool? IsDefault { get; set; }

        public bool Success { get; set; }

        protected XElement CreateNodeAdd(string key, string value)
        {
            var add = new XElement("add");
            add.SetAttributeValue("key", key);
            add.SetAttributeValue("value", value);
            return add;
        }

        protected XElement SetNodeBroker(XElement brokerNode, string name, string type)
        {
            brokerNode.RemoveNodes();

            if (brokerNode.Attribute("name") == null)
                brokerNode.SetAttributeValue("name", name);

            brokerNode.SetAttributeValue("type", type);

            return brokerNode;
        }

        protected void CreateNewXml(string filePath)
        {
            var baseXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <configSections>
    <section name=""MessagingConfigSection"" type=""Benner.Messaging.MessagingFileConfigSection, Benner.Messaging, Culture=neutral, PublicKeyToken=257abf4668fbf313"" />
  </configSections>
  <MessagingConfigSection>
    <!--Benner.Messaging is multi broker, choose the default-->
    <brokerList default="""">
    </brokerList>    
    <!--specify only queues that should use other than default broker-->
    <queues>
    </queues>
  </MessagingConfigSection>
</configuration>";
            File.AppendAllText(filePath, baseXml);
            Console.WriteLine("Arquivo de configuração 'messaging.config' criado com sucesso.");
        }

        protected XElement GetBrokerList(XDocument xml)
        {
            return xml.Element("configuration").Element("MessagingConfigSection").Element("brokerList");
        }

        protected void BaseConfigure(string type, XElement[] adds)
        {
            if (adds.Length == 0)
            {
                Console.WriteLine("Nenhuma configuração válida foi informada. Encerrando configuração sem alterações.");
                Success = false;
                return;
            }

            if (!File.Exists(FilePath))
                CreateNewXml(FilePath);

            var xml = XDocument.Load(FilePath);
            var brokerList = GetBrokerList(xml);

            var hasSameNameBroker = brokerList.Elements("broker").Any(e => e.Attribute("name").Value == BrokerName);
            if (hasSameNameBroker)
            {
                Console.WriteLine($"Já existe um broker configurado com mesmo nome '{BrokerName}' no arquivo. Deseja sobrescrever? [s/n]");
                var answer = Console.ReadLine();
                switch (answer.ToLower())
                {
                    case "n":
                        Console.WriteLine("Encerrando configuração sem alterações.");
                        Success = false;
                        return;
                    case "s":
                        var broker = brokerList.Elements("broker").First(e => e.Attribute("name").Value == BrokerName);
                        SetNodeBroker(broker, BrokerName, type);
                        broker.Add(adds);
                        break;
                    default:
                        Console.WriteLine("Opção inválida.");
                        Success = false;
                        return;
                }
            }
            else
            {
                var newBroker = SetNodeBroker(new XElement("broker"), BrokerName, type);
                newBroker.Add(adds);
                brokerList.Add(newBroker);
            }

            if (IsDefault.HasValue && IsDefault.Value)
                brokerList.SetAttributeValue("default", BrokerName);

            Success = true;
            xml.Save(FilePath, SaveOptions.None);
        }
    }
}
