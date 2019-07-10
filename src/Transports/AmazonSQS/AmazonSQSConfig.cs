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
    internal class AmazonSQSConfig : IInternalBrokerConfig
    {
        internal int InvisibilityTimeInMinutes { get; set; }

        public AmazonSQSConfig(Dictionary<string, string> configurations)
        {
            ValidateCredentialsFileExists();

            if (int.TryParse(configurations.GetValue("InvisibilityTime", true), out int invibilityTime))
                InvisibilityTimeInMinutes = invibilityTime;
            else
                throw new ArgumentException("The informed invisibility time is not a valid integer number.");
        }

        private void ValidateCredentialsFileExists()
        {
            try
            {
                var userFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile, Environment.SpecialFolderOption.None);
                if (string.IsNullOrWhiteSpace(userFolder))
                    throw new DirectoryNotFoundException("The 'Users' directory could not be found.");

                var awsFolder = Path.Combine(userFolder, ".aws");
                if (!Directory.Exists(awsFolder))
                    throw new DirectoryNotFoundException("The '.aws' directory could not be found.");

                var credentialsFile = Path.Combine(awsFolder, "credentials");
                if (!File.Exists(credentialsFile))
                    throw new FileNotFoundException("The 'credentials' file could not be found.");
            }
            catch (Exception e)
            {
                throw new AmazonSQSException("Unable to connect to AmazonSQS server", e);
            }
        }

        public BrokerTransport CreateTransporterInstance() => new AmazonSqsTransport(this);
    }
}
