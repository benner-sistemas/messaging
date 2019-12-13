using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Benner.Messaging.CLI.Tests
{
    [TestClass]
    public class AzureTests : BaseBrokerTests
    {
        [TestInitialize]
        public void Init()
        {
            ConfigAssemblyName = "Benner.Messaging.AzureQueueConfig, Benner.Messaging";
        }

        [TestMethod]
        public void ListenAzureDeveRetornarErrosDeCadaOpcaoFaltante()
        {
            var args = "listen azure".Split(' ');
            var cliConfig = CliParserFactory.CreateForListener(args);
            cliConfig.Execute();
            IEnumerable<string> mensagensErro = cliConfig.ParsingErrors.InnerExceptions.Select(err => err.Message);

            string[] expectedMsgs =
            {
                "Required option 'c, connectionString' is missing.",
                "Required option 'i, invisibilityTime' is missing.",
                "Required option 'n, consumerName' is missing."
            };

            foreach (var msg in mensagensErro)
            {
                bool contains = expectedMsgs.Contains(msg.Trim());
                Assert.IsTrue(contains);
            }

            Assert.IsTrue(cliConfig.HasParseError);
            Assert.IsFalse(cliConfig.HasValidationError);
            Assert.IsNotNull(cliConfig.ParsingErrors);
            Assert.IsNull(cliConfig.Exception);
        }

        [TestMethod]
        public void ListenAzureDeveValidarParametrosIncorretos()
        {
            var argsList = new List<string[]>()
            {
               new string[] { "listen", "azure", "-c", "connectionString", "-i", "666", "-n", " " },
               new string[] { "listen", "azure", "-c", "connectionString", "-i", "-1", "-n", "consumer" },
               new string[] { "listen", "azure", "-c", " ", "-i", "666", "-n", "consumer" },
            };

            string[] expectedMsgs =
            {
                "O parâmetro '-n/--consumerName' deve ser informado.",
                "O parâmetro '-c/--connectionString' deve ser informado.",
                "O parâmetro '-i/--invisibilityTime' deve ser maior que 0."
            };

            foreach (string[] args in argsList)
                Assert.IsTrue(expectedMsgs.Contains(GetErrorMessage(args)));
        }

        [TestMethod]
        public void ListenAzureDeveRetornarConfiguracaoValida()
        {
            var originalInvTime = 666;
            var connStr = "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;" +
                "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;" +
                "BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;" +
                "QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;";

            var args = new string[] { "listen", "azure", "-c", connStr, "-i", originalInvTime.ToString(), "-n", "Namespace.Classe" };

            var cliConfig = CliParserFactory.CreateForListener(args);
            cliConfig.Execute();

            var config = cliConfig.Configuration;
            var brokerConfig = config.GetConfigForQueue("azure");
            int invisibilityTime = GetPropertyFromType<int>("InvisibilityTimeInMinutes", brokerConfig);

            Assert.AreEqual(originalInvTime, invisibilityTime);
        }
    }
}