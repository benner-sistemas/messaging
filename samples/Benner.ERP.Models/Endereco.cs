using System.ComponentModel.DataAnnotations;

namespace Benner.ERP.Models
{
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

        public override string ToString()
        {
            return $@"Estado: {Estado}
Municipio: {Municipio}
Bairro: {Bairro}
Logradouro: {Logradouro}
Numero: {(Numero.HasValue ? Numero.Value.ToString() : "null")}
CEP: {CEP}";
        }
    }
}
