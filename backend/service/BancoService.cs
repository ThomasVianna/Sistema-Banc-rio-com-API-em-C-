using ProjetoBanco.Models;

namespace ProjetoBanco.Services
{
    public class BancoService
    {
        private readonly List<Cliente> _clientes = new();
        private int _nextId = 1;

        public IEnumerable<Cliente> GetClientes() => _clientes;

        public Cliente? GetCliente(int id) => _clientes.FirstOrDefault(c => c.Id == id);

        public Cliente CriarCliente(Cliente novo)
        {
            novo.Id = _nextId++;
            _clientes.Add(novo);
            return novo;
        }

        public bool Depositar(int id, decimal valor)
        {
            var cliente = GetCliente(id);
            if (cliente == null || valor <= 0) return false;

            cliente.Saldo += valor;
            cliente.Historico.Add(new Transacao { Tipo = "Depósito", Valor = valor });
            return true;
        }

        public bool Sacar(int id, decimal valor)
        {
            var cliente = GetCliente(id);
            if (cliente == null || valor <= 0) return false;

            if (cliente.Saldo - valor < -cliente.LimiteCredito)
                return false;

            cliente.Saldo -= valor;
            cliente.Historico.Add(new Transacao { Tipo = "Saque", Valor = valor });
            return true;
        }

        public bool Transferir(int origemId, int destinoId, decimal valor)
        {
            if (valor <= 0) return false;

            var origem = GetCliente(origemId);
            var destino = GetCliente(destinoId);
            if (origem == null || destino == null) return false;

            if (origem.Saldo - valor < -origem.LimiteCredito)
                return false;

            origem.Saldo -= valor;
            destino.Saldo += valor;

            origem.Historico.Add(new Transacao { Tipo = $"Transferência enviada para {destino.Nome}", Valor = valor });
            destino.Historico.Add(new Transacao { Tipo = $"Transferência recebida de {origem.Nome}", Valor = valor });

            return true;
        }
    }
}
