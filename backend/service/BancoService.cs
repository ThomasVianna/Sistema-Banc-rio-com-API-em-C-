using ProjetoBanco.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoBanco.Services
{
    public class BancoService
    {
        private readonly List<Cliente> _clientes = new();
        private readonly List<Transacao> _transacoes = new();
        private int _nextClienteId = 1;
        private int _nextTransacaoId = 1;

        public IEnumerable<Cliente> GetClientes()
        {
            return _clientes.AsReadOnly();
        }

        public Cliente? GetCliente(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O ID do cliente deve ser maior que zero.", nameof(id));
            return _clientes.FirstOrDefault(c => c.Id == id);
        }

        public Cliente CriarCliente(Cliente novo)
        {
            if (novo == null)
                throw new ArgumentNullException(nameof(novo));
            if (string.IsNullOrEmpty(novo.CPF) || !ValidarCPF(novo.CPF))
                throw new ArgumentException("CPF inválido.", nameof(novo.CPF));
            if (_clientes.Any(c => c.CPF == novo.CPF))
                throw new InvalidOperationException("CPF já cadastrado.");

            novo.Id = _nextClienteId++;
            _clientes.Add(novo);
            return novo;
        }

        public async Task<bool> DepositarAsync(int id, decimal valor)
        {
            if (id <= 0)
                throw new ArgumentException("O ID do cliente deve ser maior que zero.", nameof(id));
            if (valor <= 0)
                throw new ArgumentException("O valor do depósito deve ser maior que zero.", nameof(valor));

            var cliente = GetCliente(id);
            if (cliente == null)
                return false;

            cliente.Saldo += valor;
            var transacao = new Transacao
            {
                Id = _nextTransacaoId++,
                Tipo = "Depósito",
                Valor = valor,
                Data = DateTime.UtcNow,
                ClienteId = id
            };
            cliente.Historico.Add(transacao);
            _transacoes.Add(transacao);

            return true;
        }

        public async Task<bool> SacarAsync(int id, decimal valor)
        {
            if (id <= 0)
                throw new ArgumentException("O ID do cliente deve ser maior que zero.", nameof(id));
            if (valor <= 0)
                throw new ArgumentException("O valor do saque deve ser maior que zero.", nameof(valor));

            var cliente = GetCliente(id);
            if (cliente == null || cliente.Saldo + cliente.LimiteCredito < valor)
                return false;

            cliente.Saldo -= valor;
            var transacao = new Transacao
            {
                Id = _nextTransacaoId++,
                Tipo = "Saque",
                Valor = valor,
                Data = DateTime.UtcNow,
                ClienteId = id
            };
            cliente.Historico.Add(transacao);
            _transacoes.Add(transacao);

            return true;
        }

        public async Task<bool> TransferirAsync(int origemId, int destinoId, decimal valor)
        {
            if (origemId <= 0 || destinoId <= 0)
                throw new ArgumentException("Os IDs de origem e destino devem ser maiores que zero.");
            if (origemId == destinoId)
                throw new ArgumentException("As contas de origem e destino não podem ser iguais.");
            if (valor <= 0)
                throw new ArgumentException("O valor da transferência deve ser maior que zero.", nameof(valor));

            var origem = GetCliente(origemId);
            var destino = GetCliente(destinoId);
            if (origem == null || destino == null || origem.Saldo + origem.LimiteCredito < valor)
                return false;

            origem.Saldo -= valor;
            destino.Saldo += valor;

            var transacaoSaida = new Transacao
            {
                Id = _nextTransacaoId++,
                Tipo = "Transferência",
                Valor = valor,
                Data = DateTime.UtcNow,
                ClienteId = origemId,
                ClienteDestinoId = destinoId
            };
            var transacaoEntrada = new Transacao
            {
                Id = _nextTransacaoId++,
                Tipo = "Depósito",
                Valor = valor,
                Data = DateTime.UtcNow,
                ClienteId = destinoId,
                ClienteDestinoId = origemId
            };

            origem.Historico.Add(transacaoSaida);
            destino.Historico.Add(transacaoEntrada);
            _transacoes.Add(transacaoSaida);
            _transacoes.Add(transacaoEntrada);

            return true;
        }

        private bool ValidarCPF(string cpf)
        {
            cpf = cpf.Replace(".", "").Replace("-", "");
            if (cpf.Length != 11 || !cpf.All(char.IsDigit))
                return false;

            // Lógica simplificada de validação de CPF
            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicadores1[i];
            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf += digito;
            soma = 0;

            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicadores2[i];
            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito += resto.ToString();
            return cpf.EndsWith(digito);
        }
    }
}