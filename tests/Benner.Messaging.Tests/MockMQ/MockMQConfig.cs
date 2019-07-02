using Benner.Messaging.Interfaces;
using System.Collections.Generic;

namespace Benner.Messaging.Tests
{
    public class MockMQConfig : IBrokerConfig
    {
        public MockMQConfig(Dictionary<string, string> configurations)
        {
        }

        public BrokerTransport CreateTransporterInstance() => new MockMQTransport(this);
    }
}
