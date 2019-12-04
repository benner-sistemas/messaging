using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Benner.Messaging.CLI.Tests
{
    [TestClass]
    public class CliParserTests
    {
        [TestMethod]
        public void LinhaSemComandoDeveRetornarMensagemDeNaoEncontrado()
        {
            var args = "".Split(' ');
            var cliConfig = new CliConfiguration(args);
            cliConfig.Execute();

            Assert.IsTrue(cliConfig.ParsingErrors.Message.StartsWith("Comando n�o encontrado."));
            Assert.IsTrue(cliConfig.HasParseError);
            Assert.IsFalse(cliConfig.HasValidationError);
            Assert.IsNotNull(cliConfig.ParsingErrors);
            Assert.IsNull(cliConfig.Exception);
        }

        [TestMethod]
        public void LinhaComListenDeveContinuarERetornarMensagem()
        {
            var args = "listen".Split(' ');
            var cliConfig = new CliConfiguration(args);
            cliConfig.Execute();
            var msg = cliConfig.ParsingErrors.InnerExceptions.Select(err => err.Message).First();
            Assert.AreEqual(" No verb selected.", msg);
            Assert.IsTrue(cliConfig.HasParseError);
            Assert.IsFalse(cliConfig.HasValidationError);
            Assert.IsNotNull(cliConfig.ParsingErrors);
            Assert.IsNull(cliConfig.Exception);
        }
    }
}