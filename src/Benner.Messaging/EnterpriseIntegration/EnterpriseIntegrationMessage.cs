using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Benner.Messaging
{
    public class EnterpriseIntegrationMessage
    {
        public EnterpriseIntegrationMessage() => ExceptionList = new List<Exception>();

        public string MessageID { get; set; }

        public object Body { get; set; }

        public int RetryCount { get; set; }

        public DateTime WaitUntil { get; set; }

        public List<Exception> ExceptionList { get; set; }

        public static EnterpriseIntegrationMessage Create(IEnterpriseIntegrationResquest resquest)
        {
            return new EnterpriseIntegrationMessage()
            {
                Body = JsonConvert.SerializeObject(resquest),
                MessageID = resquest.RequestID.ToString(),
            };
        }
    }
}