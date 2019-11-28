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
            var cliConfig = new CliConfiguration(args);
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
            };

            string[] expectedMsgs =
            {
                "O parâmetro '-n/--consumerName' deve ser informado.",
                "O parâmetro '-i/--invisibilityTime' deve ser maior que 0."
            };

            foreach (string[] args in argsList)
                Assert.IsTrue(expectedMsgs.Contains(GetErrorMessage(args)));
        }

        [TestMethod]
        public void ListenAmazonDeveRetornarConfiguracaoValida()
        {
            var originalInvTime = 666;
            var args = new string[] { "listen", "amazon", "-i", originalInvTime.ToString(), "-n", "Namespace.Classe" };

            var cliConfig = new CliConfiguration(args);
            cliConfig.Execute();

            var config = cliConfig.Configuration;
            // O amazon precisa de configurações que só são suportadas por arquivo e não pela classe builder
            // Aqui será injetado o necessário via reflection para finalizar o teste de Assert
            var configType = typeof(MessagingConfig);
            FieldInfo settingsField = configType.GetField("_brokerSettingsByBrokerName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
            var settingsDic = (Dictionary<string, Dictionary<string, string>>)settingsField.GetValue(config);
            settingsDic["amazon"].Add("AccessKeyId", "AccessKeyIdValue");
            settingsDic["amazon"].Add("SecretAccessKey", "SecretAccessKeyValue");
            //

            var brokerConfig = config.GetConfigForQueue("amazon");
            int invisibilityTime = GetPropertyFromType<int>("InvisibilityTimeInMinutes", brokerConfig);

            Assert.AreEqual(originalInvTime, invisibilityTime);
        }
    }
}