using Microsoft.AspNetCore.Mvc;
using ProjetoBanco.Models;
using ProjetoBanco.Services;
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

        [HttpGet("clientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetClientes()
        {
            var clientes = _bancoService.GetClientes();
            return Ok(clientes);
        }

        [HttpGet("clientes/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCliente(int id)
        {
            if (id <= 0)
                return BadRequest(new { Mensagem = "O ID do cliente deve ser maior que zero." });

            var cliente = _bancoService.GetCliente(id);
            if (cliente == null)
                return NotFound(new { Mensagem = $"Cliente com ID {id} não encontrado." });

            return Ok(cliente);
        }

        [HttpPost("clientes")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CriarCliente([FromBody] Cliente cliente)
        {
            if (cliente == null || !ModelState.IsValid)
                return BadRequest(new { Mensagem = "Dados do cliente inválidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            try
            {
                var novo = _bancoService.CriarCliente(cliente);
                return CreatedAtAction(nameof(GetCliente), new { id = novo.Id }, new
                {
                    Id = novo.Id,
                    Nome = novo.Nome,
                    CPF = novo.CPF,
                    Saldo = novo.Saldo,
                    Mensagem = "Cliente criado com sucesso."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [HttpPost("deposito/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Depositar(int id, [FromBody] TransacaoRequest request)
        {
            if (id <= 0)
                return BadRequest(new { Mensagem = "O ID do cliente deve ser maior que zero." });

            if (request == null || request.Valor <= 0)
                return BadRequest(new { Mensagem = "O valor do depósito deve ser maior que zero." });

            var cliente = _bancoService.GetCliente(id);
            if (cliente == null)
                return NotFound(new { Mensagem = $"Cliente com ID {id} não encontrado." });

            try
            {
                var sucesso = _bancoService.Depositar(id, request.Valor);
                if (!sucesso)
                    return BadRequest(new { Mensagem = "Erro ao realizar o depósito." });

                var clienteAtualizado = _bancoService.GetCliente(id);
                return Ok(new { Mensagem = $"Depósito de R${request.Valor:F2} realizado com sucesso.", SaldoAtual = clienteAtualizado.Saldo });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [HttpPost("saque/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Sacar(int id, [FromBody] TransacaoRequest request)
        {
            if (id <= 0)
                return BadRequest(new { Mensagem = "O ID do cliente deve ser maior que zero." });

            if (request == null || request.Valor <= 0)
                return BadRequest(new { Mensagem = "O valor do saque deve ser maior que zero." });

            var cliente = _bancoService.GetCliente(id);
            if (cliente == null)
                return NotFound(new { Mensagem = $"Cliente com ID {id} não encontrado." });

            if (cliente.Saldo == 0)
                return BadRequest(new { Mensagem = "Não é possível realizar saque com saldo zero." });

            if (cliente.Saldo < request.Valor)
                return BadRequest(new { Mensagem = "Saldo insuficiente para o saque." });

            try
            {
                var sucesso = _bancoService.Sacar(id, request.Valor);
                if (!sucesso)
                    return BadRequest(new { Mensagem = "Erro ao realizar o saque." });

                var clienteAtualizado = _bancoService.GetCliente(id);
                return Ok(new { Mensagem = $"Saque de R${request.Valor:F2} realizado com sucesso.", SaldoAtual = clienteAtualizado.Saldo });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        [HttpPost("transferir")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Transferir([FromBody] TransferenciaRequest request)
        {
            if (request == null || !ModelState.IsValid)
                return BadRequest(new { Mensagem = "Dados da transferência inválidos.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            if (request.OrigemId <= 0 || request.DestinoId <= 0)
                return BadRequest(new { Mensagem = "Os IDs de origem e destino devem ser maiores que zero." });

            if (request.OrigemId == request.DestinoId)
                return BadRequest(new { Mensagem = "As contas de origem e destino não podem ser iguais." });

            if (request.Valor <= 0)
                return BadRequest(new { Mensagem = "O valor da transferência deve ser maior que zero." });

            var origem = _bancoService.GetCliente(request.OrigemId);
            var destino = _bancoService.GetCliente(request.DestinoId);

            if (origem == null)
                return NotFound(new { Mensagem = $"Conta de origem com ID {request.OrigemId} não encontrada." });
            if (destino == null)
                return NotFound(new { Mensagem = $"Conta de destino com ID {request.DestinoId} não encontrada." });

            if (origem.Saldo == 0)
                return BadRequest(new { Mensagem = "Não é possível realizar transferência com saldo zero na conta de origem." });

            if (origem.Saldo < request.Valor)
                return BadRequest(new { Mensagem = "Saldo insuficiente para a transferência." });

            try
            {
                var sucesso = _bancoService.Transferir(request.OrigemId, request.DestinoId, request.Valor);
                if (!sucesso)
                    return BadRequest(new { Mensagem = "Erro ao realizar a transferência." });

                var origemAtualizada = _bancoService.GetCliente(request.OrigemId);
                var destinoAtualizado = _bancoService.GetCliente(request.DestinoId);
                return Ok(new
                {
                    Mensagem = $"Transferência de R${request.Valor:F2} realizada com sucesso.",
                    SaldoOrigem = origemAtualizada.Saldo,
                    SaldoDestino = destinoAtualizado.Saldo
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }
    }

    public class TransacaoRequest
    {
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