using Benner.Messaging.CLI.Extensions;
using Benner.Messaging.CLI.Verbs.Listener;
using Benner.Messaging.Interfaces;
using CommandLine;
using CommandLine.Text;
using System;
using System.Linq;

namespace Benner.Messaging.CLI
{
    public class CliParser
    {
        public string Consumer { get; private set; }

        public string BrokerName { get; private set; }

        public IMessagingConfig Configuration { get; private set; }

        public bool HasValidationError { get; private set; }

        public bool HasParseError { get; private set; }

        public Exception Exception { get; private set; }

        public AggregateException ParsingErrors { get; private set; }

        private readonly string[] _args;

        public CliParser(string[] args)
        {
            _args = args;
        }

        public void Parse()
        {
            try
            {
                var parsed = new Parser(p => p.HelpWriter = null)
                    .ParseVerbs(_args, typeof(ListenerVerb))
                    .WithParsed(p => OnParseSuccess(p as ListenerVerb));
                parsed.WithNotParsed(e => OnParseError(parsed));
            }
            catch (Exception e)
            {
                HasParseError = true;
                HasValidationError = false;
                ParsingErrors = new AggregateException("Comando não encontrado.", e);
                throw new ArgumentNullException("Comando não encontrado.", e);
            }
        }

        private void OnParseError<T>(ParserResult<T> result)
        {
            var help = HelpText.AutoBuild(result, h =>
            {
                h.AdditionalNewLineAfterOption = false;
                h.AutoVersion = false;
                h.AddPostOptionsLine("");

                return h;
            }, 120);

            Console.WriteLine(help);

            var builder = SentenceBuilder.Create();
            var errorMessages = HelpText.RenderParsingErrorsTextAsLines(result, builder.FormatError, builder.FormatMutuallyExclusiveSetErrors, 1);

            HasValidationError = false;
            HasParseError = true;

            var exceptions = errorMessages.Select(msg => new ArgumentException(msg));
            ParsingErrors = new AggregateException("Ocorreu um erro na conversão da linha de comando em configuração.", exceptions);
        }

        private void OnParseSuccess(ListenerVerb arg)
        {
            Consumer = arg.Consumer;

            BrokerName = arg.BrokerName;
            try
            {
                Configuration = arg.GetConfiguration();
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
