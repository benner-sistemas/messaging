using System;

namespace Benner.Messaging
{
    public interface IEnterpriseIntegrationRequest
    {
        Guid? RequestID { get; set; }
    }
}