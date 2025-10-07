using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProjetoBanco.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 dígitos numéricos.")]
        public string CPF { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
        public decimal Saldo { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "O limite de crédito não pode ser negativo.")]
        public decimal LimiteCredito { get; set; } = 500;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Transacao> Historico { get; set; } = new();
    }
}