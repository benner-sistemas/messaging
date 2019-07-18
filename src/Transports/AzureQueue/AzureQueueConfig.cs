using System;
using System.Collections.Generic;
using Microsoft.Azure.Storage;
using Benner.Messaging.Interfaces;

namespace Benner.Messaging
{
    /// <summary>
    /// Classe de configuração do Azure Queue.
    /// </summary>
    internal class AzureQueueConfig : IInternalBrokerConfig
    {
        internal CloudStorageAccount StorageAccount { get; set; }
        internal int InvisibilityTimeInMinutes { get; set; }

        public AzureQueueConfig(Dictionary<string, string> configurations)
        {
            string connectionString = configurations.GetValue("ConnectionString", true);
            CloudStorageAccount.TryParse(connectionString, out CloudStorageAccount storageAccount);
            StorageAccount = storageAccount ?? throw new ArgumentException("Invalid ConnectionString.");

            if (int.TryParse(configurations.GetValue("InvisibilityTime", true), out int invibilityTime))
                InvisibilityTimeInMinutes = invibilityTime;
            else
                throw new ArgumentException("The informed invisibility time is not a valid integer number.");
        }

        public BrokerTransport CreateTransporterInstance() => new AzureQueueTransport(this);
    }
}
