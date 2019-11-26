using Benner.Messaging.Common.Extensions;
using Benner.Messaging.Common.Verbs;
using Benner.Messaging.Interfaces;
using CommandLine;
using System;
using System.Collections.Generic;

namespace Benner.Messaging.Common
{
    public class CliConfiguration
    {
        public string Consumer { get; private set; }
        public IMessagingConfig Configuration { get; private set; }
        public bool IsError { get; private set; }
        public Exception Exception { get; private set; }

        public CliConfiguration(string[] args)
        {
            Parser.Default.ParseVerbs(args, typeof(ListenVerb)).WithParsed(OnCommandExecute).WithNotParsed(OnError);
        }

        private void OnError(IEnumerable<Error> obj) => IsError = true;

        private void OnCommandExecute(object arg)
        {
            try
            {
                var result = (ListenVerb)arg;
                Configuration = result.GetConfiguration();
                Consumer = result.Consumer;
            }
            catch (Exception e)
            {
                IsError = true;
                Exception = e;
            }
        }
    }
}
