using Benner.Messaging.Common.Extensions;
using Benner.Messaging.Common.Verbs;
using Benner.Messaging.Interfaces;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Common
{
    public class CliConfiguration
    {
        public string Consumer { get; private set; }

        public IMessagingConfig Configuration { get; private set; }

        private readonly string[] _args;

        public CliConfiguration(string[] args) => _args = args;

        public void Execute()
        {
            var parsed = Parser.Default.ParseVerbs(_args, typeof(ListenVerb));
            parsed.WithParsed(OnCommandExecute);
            parsed.WithNotParsed(errors => ThrowOnParseError(parsed, errors));
        }

        private void ThrowOnParseError<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var builder = SentenceBuilder.Create();
            var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

            throw new ArgumentException("Ocorreu um erro na conversão da linha de comando em configuração.\r\n" 
                + string.Join("\r\n", errorMessages));
        }

        private void OnCommandExecute(object arg)
        {
            try
            {
                var result = (ListenVerb)arg;
                Consumer = result.Consumer;
                Configuration = result.GetConfiguration();
            }
            catch (ArgumentException e)
            {
                throw new InvalidOperationException("Falha na validação de parâmetros", e);
            }
        }
    }
}
