using Benner.Messaging.CLI.Extensions;
using Benner.Messaging.CLI.Verbs;
using Benner.Messaging.Interfaces;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Benner.Messaging.CLI
{
    public class CliConfiguration
    {
        public string Consumer { get; private set; }

        public IMessagingConfig Configuration { get; private set; }

        public bool HasValidationError { get; private set; }

        public bool HasParseError { get; private set; }

        public Exception Exception { get; private set; }

        public AggregateException ParsingErrors { get; private set; }

        private readonly string[] _args;

        public CliConfiguration(string[] args) => _args = args;

        public void Execute()
        {
            var parsed = Parser.Default.ParseVerbs(_args, typeof(ListenVerb));
            parsed.WithParsed(OnParseSuccess);
            parsed.WithNotParsed(errors => OnParseError(parsed, errors));
        }

        private void OnParseError<T>(ParserResult<T> result, IEnumerable<Error> errors)
        {
            var builder = SentenceBuilder.Create();
            var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

            HasValidationError = false;
            HasParseError = true;

            var exceptions = errorMessages.Select(msg => new ArgumentException(msg));
            ParsingErrors = new AggregateException("Ocorreu um erro na conversão da linha de comando em configuração.", exceptions);
        }

        private void OnParseSuccess(object arg)
        {
            try
            {
                var result = (ListenVerb)arg;
                Consumer = result.Consumer;
                Configuration = result.GetConfiguration();
            }
            catch (ArgumentException e)
            {
                HasValidationError = true;
                HasParseError = false;
                Exception = e;
            }
            catch (Exception e)
            {
                HasValidationError = false;
                HasParseError = false;
                Exception = e;
            }
        }
    }
}
