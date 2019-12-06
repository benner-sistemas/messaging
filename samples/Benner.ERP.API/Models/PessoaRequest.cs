using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Benner.Messaging;

namespace Benner.ERP.API.Models
{
    public class PessoaRequest : IEnterpriseIntegrationResquest
    {
        [Key]
        public Guid? RequestID { get; set; }

        [Required]
        [StringLength(14)]
        public string CPF { get; set; }

        [Required]
        public string Nome { get; set; }

        [Required]
        public DateTime? Nascimento { get; set; }

        [Required]
        public Endereco Endereco { get; set; }
    }

    public class Endereco
    {
        [Required]
        public string Logradouro { get; set; }

        [Required]
        public int? Numero { get; set; }

        [Required]
        [StringLength(9)]
        public string CEP { get; set; }

        [Required]
        public string Bairro { get; set; }

        [Required]
        public string Municipio { get; set; }

        [Required]
        public string Estado { get; set; }
    }
}
