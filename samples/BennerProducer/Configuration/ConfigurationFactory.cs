using Benner.Messaging;
using System;
using System.Linq;

namespace BennerProducer.Configuration
{
    public class ConfigurationFactory
    {
        public IConfiguration GetConfiguration(string[] args)
        {
            var brokerType = GetBrokerType(args);

            switch (brokerType)
            {
                case BrokerType.ActiveMQ:
                    return new ConfigurationActiveMq(args);
                case BrokerType.AmazonSQS:
                    return new ConfigurationAmazonSqs(args);
                case BrokerType.AzureQueue:
                    return new ConfigurationAzureQueue(args);
                case BrokerType.RabbitMQ:
                    return new ConfigurationRabbitMq(args);
                default:
                    throw new NotSupportedException("Broker not supported");
            }
        }

        private BrokerType GetBrokerType(string[] args)
        {
            var parameterBroker = args.FirstOrDefault(x => x.ToLower().Contains("broker"))?.Split(':');

            if (parameterBroker == null ||
                parameterBroker.Count() < 2)
            {
                throw new NotSupportedException("Broker invalid");
            }

            object broker;

            if (!Enum.TryParse(typeof(BrokerType), parameterBroker[1], out broker))
            {
                throw new NotSupportedException("Broker invalid");
            }

            return (BrokerType)broker;
        }
    }
}