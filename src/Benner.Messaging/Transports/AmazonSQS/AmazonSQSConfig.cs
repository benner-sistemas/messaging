using Benner.Messaging.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using static System.Environment;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração do Amazon SQS.
    /// </summary>
    internal class AmazonSQSConfig : IInternalBrokerConfig
    {
        internal int InvisibilityTimeInMinutes { get; set; }
        internal string AccessKeyId { get; set; }
        internal string SecretAccessKey { get; set; }
        internal bool CredentialFileExists { get; set; }

        public AmazonSQSConfig(Dictionary<string, string> configurations)
        {
            CredentialFileExists = ValidateCredentialsFileExists();
            if (!CredentialFileExists)
            {
                AccessKeyId = configurations.GetValue("AccessKeyId", true);
                SecretAccessKey = configurations.GetValue("SecretAccessKey", true);
            }

            if (int.TryParse(configurations.GetValue("InvisibilityTime", true), out int invibilityTime))
                InvisibilityTimeInMinutes = invibilityTime;
            else
                throw new ArgumentException("The informed invisibility time is not a valid integer number.");
        }

        private bool ValidateCredentialsFileExists()
        {
            var userFolder = Environment.GetFolderPath(SpecialFolder.UserProfile, SpecialFolderOption.None);
            if (string.IsNullOrWhiteSpace(userFolder))
                return false;

            var awsFolder = Path.Combine(userFolder, ".aws");
            if (!Directory.Exists(awsFolder))
                return false;

            var credentialsFile = Path.Combine(awsFolder, "credentials");
            if (!File.Exists(credentialsFile))
                return false;

            return true;
        }

        public BrokerTransport CreateTransporterInstance() => new AmazonSqsTransport(this);
    }
}
