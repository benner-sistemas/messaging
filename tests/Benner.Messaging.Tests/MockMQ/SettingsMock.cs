using Benner.Listener;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Benner.Messaging.Retry.Tests.Mocks
{
    public class SettingsMock : IEnterpriseIntegrationSettings
    {
        public string QueueName => "nathank-teste";

        public int RetryLimit => 1;

        public long RetryIntervalInMilliseconds => 10;
    }
}
