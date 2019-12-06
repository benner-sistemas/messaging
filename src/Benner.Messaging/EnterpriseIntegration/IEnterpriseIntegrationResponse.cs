using System;

namespace Benner.Messaging
{
    public interface IEnterpriseIntegrationResponse
    {
        Guid? MessageID { get; set; }
        string QueueName { get; set; }
        DateTime? CreatedAt { get; set; }
    }
}