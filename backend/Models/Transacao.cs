using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoBanco.Models
{
    public class Transacao
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O tipo da transação é obrigatório.")]
        [StringLength(20, ErrorMessage = "O tipo da transação deve ter no máximo 20 caracteres.")]
        [RegularExpression("^(Depósito|Saque|Transferência)$", ErrorMessage = "O tipo da transação deve ser 'Depósito', 'Saque' ou 'Transferência'.")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O valor da transação é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "A data da transação é obrigatória.")]
        public DateTime Data { get; set; } = DateTime.UtcNow;

        public int ClienteId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ClienteDestinoId { get; set; }

        [JsonIgnore]
        public Cliente? Cliente { get; set; }

        [JsonIgnore]
        public Cliente? ClienteDestino { get; set; }
    }
}