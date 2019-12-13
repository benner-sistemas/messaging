using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Benner.Messaging.CLI.Tests
{
    [TestClass]
    public class ActiveTests : BaseBrokerTests
    {
        [TestInitialize]
        public void Init()
        {
            ConfigAssemblyName = "Benner.Messaging.ActiveMQConfig, Benner.Messaging";
        }

        [TestMethod]
        public void ListenActiveDeveRetornarErrosDeCadaOpcaoFaltante()
        {
            var args = "listen active".Split(' ');
            var cliConfig = CliParserFactory.CreateForListener(args);
            cliConfig.Execute();
            IEnumerable<string> mensagensErro = cliConfig.ParsingErrors.InnerExceptions.Select(err => err.Message);

            string[] expectedMsgs =
            {
                "Required option 'h, hostName' is missing.",
                "Required option 'port' is missing.",
                "Required option 'u, user' is missing.",
                "Required option 'p, password' is missing.",
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
        public void ListenActiveDeveValidarParametrosIncorretos()
        {
            var argsList = new List<string[]>()
            {
               new string[] { "listen", "active", "-h", "host", "--port", "1", "-u", "user", "-p", "password", "-n", " " },
               new string[] { "listen", "active", "-h", "host", "--port", "1", "-u", "user", "-p", " ", "-n", "consumer" },
               new string[] { "listen", "active", "-h", "host", "--port", "1", "-u", " ", "-p", "password", "-n", "consumer" },
               new string[] { "listen", "active", "-h", "host", "--port", "-1", "-u", "user", "-p", "password", "-n", "consumer" },
               new string[] { "listen", "active", "-h", " ", "--port", "1", "-u", "user", "-p", "password", "-n", "consumer" }
            };

            string[] expectedMsgs =
            {
                "O parâmetro '-n/--consumerName' deve ser informado.",
                "O parâmetro '-p/--password' deve ser informado.",
                "O parâmetro '-u/--user' deve ser informado.",
                "O parâmetro '--port' deve ser maior que 0.",
                "O parâmetro '-h/--hostName' deve ser informado."
            };

            foreach (string[] args in argsList)
                Assert.IsTrue(expectedMsgs.Contains(GetErrorMessage(args)));
        }

        [TestMethod]
        public void ListenActiveDeveRetornarConfiguracaoValida()
        {
            var originalHost = "bnu-vtec00x";
            var originalUser = "username";
            var originalPass = "senha";
            var originalPort = 666;
            var args = new string[] { "listen", "active", "-h", originalHost, "--port",
                originalPort.ToString(), "-u", originalUser, "-p", originalPass, "-n", "Namespace.Classe" };

            var cliConfig = CliParserFactory.CreateForListener(args);
            cliConfig.Execute();

            var config = cliConfig.Configuration;
            var brokerConfig = config.GetConfigForQueue("active");

            string host = GetPropertyFromType<string>("HostName", brokerConfig);
            string user = GetPropertyFromType<string>("UserName", brokerConfig);
            string pass = GetPropertyFromType<string>("Password", brokerConfig);
            int port = GetPropertyFromType<int>("Port", brokerConfig);

            Assert.AreEqual(originalHost, host);
            Assert.AreEqual(originalUser, user);
            Assert.AreEqual(originalPass, pass);
            Assert.AreEqual(originalPort, port);
        }
    }
}