﻿using Benner.Messaging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Benner.ERP.Models
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

        public override string ToString()
        {
            return $@"RequestID: {RequestID.Value}
Nome: {Nome}
CPF: {CPF}
Nascimento: {(Nascimento.HasValue ? Nascimento.Value.ToLongDateString() : "null")}
Endereço: {Endereco.ToString()}";
        }
    }
}