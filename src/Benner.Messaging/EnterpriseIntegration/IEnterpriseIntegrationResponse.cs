using System;

namespace Benner.Messaging
{
    public interface IEnterpriseIntegrationResponse
    {
        Guid? MessgeID { get; set; }
        string QueueName { get; set; }
        DateTime? CreatedAt { get; set; }
    }
}