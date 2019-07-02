using Amazon.SQS;
using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração do Amazon SQS.
    /// </summary>
    internal class AmazonSqsConfig : IInternalBrokerConfig
    {
        internal int InvisibilityTimeInMinutes { get; set; }

        public AmazonSqsConfig(Dictionary<string, string> configurations)
        {
            ValidateCredentialsFileExists();

            if (int.TryParse(configurations.GetValue("InvisibilityTime", true), out int invibilityTime))
                InvisibilityTimeInMinutes = invibilityTime;
            else
                throw new ArgumentException("O tempo de invisibilidade informado não é um valor inteiro válido");
        }

        private void ValidateCredentialsFileExists()
        {
            try
            {
                var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.None);
                if (string.IsNullOrWhiteSpace(userFolder))
                    throw new DirectoryNotFoundException("Diretório de usuários não encontrado.");

                var awsFolder = Path.Combine(userFolder, ".aws");
                if (!Directory.Exists(awsFolder))
                    throw new DirectoryNotFoundException("Diretório '.aws' não encontrado.");

                var credentialsFile = Path.Combine(awsFolder, "credentials");
                if (!File.Exists(credentialsFile))
                    throw new FileNotFoundException("Arquivo 'credentials' não encontrado.");
            }
            catch (Exception e)
            {
                throw new AmazonSQSException("Não foi possível configurar o serviço AmazonSQS.", e);
            }
        }

        public BrokerTransport CreateTransporterInstance() => new AmazonSqsTransport(this);
    }
}
