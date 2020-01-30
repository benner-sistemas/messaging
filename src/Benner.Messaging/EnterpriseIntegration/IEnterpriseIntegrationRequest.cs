using System;
using System.ComponentModel.DataAnnotations;

namespace Benner.Messaging
{
    public interface IEnterpriseIntegrationRequest
    {
        /// <summary>
        /// Indentificador único desta requisição
        /// </summary>
        [Required]
        Guid? RequestID { get; set; }
    }
}