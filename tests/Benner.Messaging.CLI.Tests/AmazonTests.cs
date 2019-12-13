using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Benner.Messaging.CLI.Tests
{
    [TestClass]
    public class AmazonTests : BaseBrokerTests
    {
        [TestInitialize]
        public void Init()
        {
            ConfigAssemblyName = "Benner.Messaging.AmazonSQSConfig, Benner.Messaging";
        }

        [TestMethod]
        public void ListenAmazonDeveRetornarErrosDeCadaOpcaoFaltante()
        {
            var args = "listen amazon".Split(' ');
            var cliConfig = CliParserFactory.CreateForListener(args);
            cliConfig.Execute();
            IEnumerable<string> mensagensErro = cliConfig.ParsingErrors.InnerExceptions.Select(err => err.Message);

            string[] expectedMsgs =
            {
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
        public void ListenAmazonDeveValidarParametrosIncorretos()
        {
            var argsList = new List<string[]>()
            {
               new string[] { "listen", "amazon", "-i", "666", "-n", " " },
               new string[] { "listen", "amazon", "-i", "-1", "-n", "consumer" },
               new string[] { "listen", "amazon", "-i", "666", "-n", "consumer", "-a", "accessKeyId" },
               new string[] { "listen", "amazon", "-i", "666", "-n", "consumer", "-s", "secretAccessKey"},
            };

            string[] expectedMsgs =
            {
                "O parâmetro '-n/--consumerName' deve ser informado.",
                "O parâmetro '-i/--invisibilityTime' deve ser maior que 0.",
                "O parâmetro 'accessKeyId' ou 'secretAccessKey' são obrigatórios caso um deles seja informado."
            };

            foreach (string[] args in argsList)
                Assert.IsTrue(expectedMsgs.Contains(GetErrorMessage(args)));
        }

        [TestMethod]
        public void ListenAmazonDeveRetornarConfiguracaoValida()
        {
            var originalInvTime = 666;
            var originalAccessKeyId = "AccessKeyIdValue";
            var originalSecretAccessKey = "SecretAccessKeyValue";
            var args = new string[] { "listen", "amazon", "-i", originalInvTime.ToString(), "-n", "Namespace.Classe",
                "-a", originalAccessKeyId, "-s", originalSecretAccessKey };

            var cliConfig = CliParserFactory.CreateForListener(args);
            cliConfig.Execute();

            var config = cliConfig.Configuration;

            var brokerConfig = config.GetConfigForQueue("amazon");
            int invisibilityTime = GetPropertyFromType<int>("InvisibilityTimeInMinutes", brokerConfig);
            string accessKeyId = GetPropertyFromType<string>("AccessKeyId", brokerConfig);
            string secretAccessKey = GetPropertyFromType<string>("SecretAccessKey", brokerConfig);

            Assert.AreEqual(originalInvTime, invisibilityTime);
            Assert.AreEqual(originalAccessKeyId, accessKeyId);
            Assert.AreEqual(originalSecretAccessKey, secretAccessKey);
        }
    }
}