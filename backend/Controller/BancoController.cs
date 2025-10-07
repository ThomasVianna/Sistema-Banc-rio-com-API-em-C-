using Microsoft.AspNetCore.Mvc;
using ProjetoBanco.Models;
using ProjetoBanco.Services;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ProjetoBanco.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BancoController : ControllerBase
    {
        private readonly BancoService _bancoService;

        public BancoController(BancoService bancoService)
        {
            _bancoService = bancoService ?? throw new ArgumentNullException(nameof(bancoService));
        }

        /// <summary>
        /// Obtém a lista de todos os clientes.
        /// </summary>
        /// <returns>Lista de clientes.</returns>
        [HttpGet("clientes")]
        public IActionResult GetClientes()
        {
            var clientes = _bancoService.GetClientes();
            return Ok(clientes);
        }

        /// <summary>
        /// Obtém um cliente específico pelo ID.
        /// </summary>
        /// <param name="id">ID do cliente.</param>
        /// <returns>Cliente encontrado ou erro 404 se não encontrado.</returns>
        [HttpGet("clientes/{id}")]
        public IActionResult GetCliente(int id)
        {
            if (id <= 0)
                return BadRequest("ID do cliente deve ser maior que zero.");

            var cliente = _bancoService.GetCliente(id);
            if (cliente == null)
                return NotFound(new { Mensagem = $"Cliente com ID {id} não encontrado." });

            return Ok(cliente);
        }

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        /// <param name="cliente">Dados do cliente a ser criado.</param>
        /// <returns>Cliente criado com o ID gerado.</returns>
        [HttpPost("clientes")]
        public IActionResult CriarCliente([FromBody] Cliente cliente)
        {
            if (cliente == null || !ModelState.IsValid)
                return BadRequest(ModelState);

            var novo = _bancoService.CriarCliente(cliente);
            return CreatedAtAction(nameof(GetCliente), new { id = novo.Id }, new
            {
                Id = novo.Id,
                Nome = novo.Nome,
                Saldo = novo.Saldo,
                Mensagem = "Cliente criado com sucesso."
            });
        }

        /// <summary>
        /// Realiza um depósito na conta de um cliente.
        /// </summary>
        /// <param name="id">ID do cliente.</param>
        /// <param name="transacao">Objeto contendo o valor do depósito.</param>
        /// <returns>Resultado da operação de depósito.</returns>
        [HttpPost("deposito")]
        public async Task<IActionResult> Depositar([FromBody] TransacaoRequest transacao)
        {
            if (transacao == null || transacao.Valor <= 0)
                return BadRequest(new { Mensagem = "O valor do depósito deve ser maior que zero." });

            var cliente = _bancoService.GetCliente(transacao.Id);
            if (cliente == null)
                return NotFound(new { Mensagem = $"Cliente com ID {transacao.Id} não encontrado." });

            var sucesso = await _bancoService.DepositarAsync(transacao.Id, transacao.Valor);
            return sucesso
                ? Ok(new { Mensagem = $"Depósito de R${transacao.Valor:F2} realizado com sucesso.", SaldoAtual = cliente.Saldo })
                : BadRequest(new { Mensagem = "Erro ao realizar o depósito." });
        }

        /// <summary>
        /// Realiza um saque na conta de um cliente.
        /// </summary>
        /// <param name="id">ID do cliente.</param>
        /// <param name="transacao">Objeto contendo o valor do saque.</param>
        /// <returns>Resultado da operação de saque.</returns>
        [HttpPost("saque")]
        public async Task<IActionResult> Sacar([FromBody] TransacaoRequest transacao)
        {
            if (transacao == null || transacao.Valor <= 0)
                return BadRequest(new { Mensagem = "O valor do saque deve ser maior que zero." });

            var cliente = _bancoService.GetCliente(transacao.Id);
            if (cliente == null)
                return NotFound(new { Mensagem = $"Cliente com ID {transacao.Id} não encontrado." });

            var sucesso = await _bancoService.SacarAsync(transacao.Id, transacao.Valor);
            return sucesso
                ? Ok(new { Mensagem = $"Saque de R${transacao.Valor:F2} realizado com sucesso.", SaldoAtual = cliente.Saldo })
                : BadRequest(new { Mensagem = "Saldo insuficiente ou valor inválido." });
        }

        /// <summary>
        /// Realiza uma transferência entre contas de dois clientes.
        /// </summary>
        /// <param name="transferencia">Objeto contendo IDs de origem, destino e valor.</param>
        /// <returns>Resultado da operação de transferência.</returns>
        [HttpPost("transferir")]
        public async Task<IActionResult> Transferir([FromBody] TransferenciaRequest transferencia)
        {
            if (transferencia == null || transferencia.Valor <= 0)
                return BadRequest(new { Mensagem = "O valor da transferência deve ser maior que zero." });

            if (transferencia.OrigemId == transferencia.DestinoId)
                return BadRequest(new { Mensagem = "As contas de origem e destino não podem ser iguais." });

            var origem = _bancoService.GetCliente(transferencia.OrigemId);
            var destino = _bancoService.GetCliente(transferencia.DestinoId);

            if (origem == null)
                return NotFound(new { Mensagem = $"Conta de origem com ID {transferencia.OrigemId} não encontrada." });
            if (destino == null)
                return NotFound(new { Mensagem = $"Conta de destino com ID {transferencia.DestinoId} não encontrada." });

            var sucesso = await _bancoService.TransferirAsync(transferencia.OrigemId, transferencia.DestinoId, transferencia.Valor);
            return sucesso
                ? Ok(new { Mensagem = $"Transferência de R${transferencia.Valor:F2} realizada com sucesso." })
                : BadRequest(new { Mensagem = "Erro na transferência. Verifique o saldo ou os dados fornecidos." });
        }
    }

    // DTOs para as requisições
    public class TransacaoRequest
    {
        [Required(ErrorMessage = "O ID do cliente é obrigatório.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
    }

    public class TransferenciaRequest
    {
        [Required(ErrorMessage = "O ID da conta de origem é obrigatório.")]
        public int OrigemId { get; set; }

        [Required(ErrorMessage = "O ID da conta de destino é obrigatório.")]
        public int DestinoId { get; set; }

        [Required(ErrorMessage = "O valor é obrigatório.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }
    }
}