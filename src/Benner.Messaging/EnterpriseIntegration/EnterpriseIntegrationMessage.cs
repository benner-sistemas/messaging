using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    public class EnterpriseIntegrationMessage
    {
        public EnterpriseIntegrationMessage() => ExceptionList = new List<Exception>();

        public string MessageID { get; set; }

        public string Body { get; set; }

        public int RetryCount { get; set; }

        public DateTime WaitUntil { get; set; }

        public List<Exception> ExceptionList { get; set; }

        public static EnterpriseIntegrationMessage Create(IEnterpriseIntegrationResquest resquest)
        {
            return new EnterpriseIntegrationMessage()
            {
                Body = Utils.SerializeObject(resquest),
                MessageID = resquest.RequestID.ToString(),
            };
        }
    }
}