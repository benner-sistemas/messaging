using System;

namespace Benner.Messaging
{
    public interface IEnterpriseIntegrationResquest
    {
        Guid? RequestID { get; set; }
    }
}