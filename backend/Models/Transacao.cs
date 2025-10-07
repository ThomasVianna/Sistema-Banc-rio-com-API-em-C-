namespace ProjetoBanco.Models
{
    public class Transacao
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty; // Depósito, Saque, Transferência
        public decimal Valor { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;
    }
}
