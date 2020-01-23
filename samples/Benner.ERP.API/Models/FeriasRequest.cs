using Benner.Messaging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Benner.ERP.API.Models
{
    public class FeriasRequest : IEnterpriseIntegrationRequest
    {
        [Key]
        public Guid? RequestID { get; set; }

        [Required]
        [StringLength(14)]
        public string CPF { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public DateTime? DataInicio { get; set; }

        [Required]
        public int QuantidadeDias { get; set; }
    }
}
