using Benner.Messaging.Common;
using Benner.Messaging.Configuration;
using System;

namespace Benner.Messaging
{
    public class EnterpriseIntegrationMessage : MessageBase
    {
        public EnterpriseIntegrationMessage() : base()
        { }

        public string MessageID { get; set; }

        public int RetryCount { get; set; }

        public DateTime WaitUntil { get; set; }

        public static EnterpriseIntegrationMessage Create(IEnterpriseIntegrationRequest request)
        {
            return new EnterpriseIntegrationMessage()
            {
                Body = JsonParser.Serialize(request),
                MessageID = request.RequestID.ToString(),
            };
        }
    }
}