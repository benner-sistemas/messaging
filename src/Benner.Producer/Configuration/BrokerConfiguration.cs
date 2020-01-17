using Benner.Messaging;
using Benner.Messaging.CLI;
using System;

namespace Benner.Producer.Configuration
{
    public class BrokerConfiguration
    {
        private string[] Args { get; set; }

        public BrokerConfiguration(string[] args)
        {
            this.Args = args;
        }

        public void SetConfiguration()
        {
            bool succeededCliConfig = false;

            if (Args.Length >= 1)
            {
                var parser = new CliParser(Args);

                parser.Parse();

                if (parser.HasValidationError ||
                    parser.Exception != null)
                {
                    throw parser.Exception;
                }

                if (parser.HasParseError)
                {
                    throw new Exception(string.Empty);
                }

                if (parser.Configuration != null)
                {
                    succeededCliConfig = true;
                }
            }
            var configFileExists = FileMessagingConfig.DefaultConfigFileExists;

            if (succeededCliConfig &&
                configFileExists)
            {
                throw new Exception("Foi detectado um arquivo de configuração. Não é possível utilizar configuração por arquivo e por linha de comando simultaneamente.");
            }

            if (!succeededCliConfig &&
                !configFileExists)
            {
                throw new Exception("Não foi detectado um arquivo de configuração.");
            }
        }
    }
}
