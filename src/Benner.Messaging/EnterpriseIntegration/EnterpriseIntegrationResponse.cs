using System;

namespace Benner.Messaging
{
    public class EnterpriseIntegrationResponse : IEnterpriseIntegrationResponse
    {
        public Guid? MessgeID { get; set; }
        public string QueueName { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}