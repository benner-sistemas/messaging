using Benner.Messaging.CLI.Extensions;
using Benner.Messaging.CLI.Verbs;
using Benner.Messaging.CLI.Verbs.Listener;
using Benner.Messaging.CLI.Verbs.Producer;
using Benner.Messaging.Interfaces;
using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
        private readonly string _obsMessage;
        private readonly Type _toRemove;

        internal CliParser(string[] args, Type toRemove, string obsMessage = null)
        {
            _toRemove = toRemove;
            _args = args;
            _obsMessage = obsMessage;
        }

        public void Parse()
        {
            try
            {
                var tipos = new List<Type> { typeof(ListenerVerb), typeof(ProducerVerb) };
                tipos.Remove(_toRemove);

                var parsed = new Parser(p => p.HelpWriter = null)
                    .ParseVerbs(_args, tipos.ToArray())
                    .WithParsed(p => OnParseSuccess(p as IBrokerVerb));
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
                if (_obsMessage != null)
                    h.AddPostOptionsLine(_obsMessage);
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

        private void OnParseSuccess(IBrokerVerb arg)
        {
            if (arg is ListenerVerb listenResult)
                Consumer = listenResult.Consumer;

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
