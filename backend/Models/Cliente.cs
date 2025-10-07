namespace ProjetoBanco.Models
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public decimal Saldo { get; set; } = 0;
        public decimal LimiteCredito { get; set; } = 500;
        public List<Transacao> Historico { get; set; } = new();
    }
}
