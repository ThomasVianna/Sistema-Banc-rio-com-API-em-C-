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

        /// <summary>
        /// Obtém a lista de todos os clientes.
        /// </summary>
        /// <returns>Lista de clientes.</returns>
        /// <response code="200">Retorna a lista de clientes.</response>
        [HttpGet("clientes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        /// <response code="200">Retorna o cliente encontrado.</response>
        /// <response code="400">ID inválido.</response>
        /// <response code="404">Cliente não encontrado.</response>
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

        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        /// <param name="cliente">Dados do cliente a ser criado.</param>
        /// <returns>Cliente criado com o ID gerado.</returns>
        /// <response code="201">Cliente criado com sucesso.</response>
        /// <response code="400">Dados inválidos ou CPF já cadastrado.</response>
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

        /// <summary>
        /// Realiza um depósito na conta de um cliente.
        /// </summary>
        /// <param name="id">ID do cliente.</param>
        /// <param name="request">Objeto contendo o valor do depósito.</param>
        /// <returns>Resultado da operação de depósito.</returns>
        /// <response code="200">Depósito realizado com sucesso.</response>
        /// <response code="400">Valor inválido.</response>
        /// <response code="404">Cliente não encontrado.</response>
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
                return sucesso
                    ? Ok(new { Mensagem = $"Depósito de R${request.Valor:F2} realizado com sucesso.", SaldoAtual = cliente.Saldo })
                    : BadRequest(new { Mensagem = "Erro ao realizar o depósito." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Realiza um saque na conta de um cliente.
        /// </summary>
        /// <param name="id">ID do cliente.</param>
        /// <param name="request">Objeto contendo o valor do saque.</param>
        /// <returns>Resultado da operação de saque.</returns>
        /// <response code="200">Saque realizado com sucesso.</response>
        /// <response code="400">Valor inválido ou saldo insuficiente.</response>
        /// <response code="404">Cliente não encontrado.</response>
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

            try
            {
                var sucesso = _bancoService.Sacar(id, request.Valor);
                return sucesso
                    ? Ok(new { Mensagem = $"Saque de R${request.Valor:F2} realizado com sucesso.", SaldoAtual = cliente.Saldo })
                    : BadRequest(new { Mensagem = "Saldo insuficiente ou valor inválido." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }

        /// <summary>
        /// Realiza uma transferência entre contas de dois clientes.
        /// </summary>
        /// <param name="request">Objeto contendo IDs de origem, destino e valor.</param>
        /// <returns>Resultado da operação de transferência.</returns>
        /// <response code="200">Transferência realizada com sucesso.</response>
        /// <response code="400">Dados inválidos ou saldo insuficiente.</response>
        /// <response code="404">Conta de origem ou destino não encontrada.</response>
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

            try
            {
                var sucesso = _bancoService.Transferir(request.OrigemId, request.DestinoId, request.Valor);
                return sucesso
                    ? Ok(new { Mensagem = $"Transferência de R${request.Valor:F2} realizada com sucesso." })
                    : BadRequest(new { Mensagem = "Erro na transferência. Verifique o saldo ou os dados fornecidos." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Mensagem = ex.Message });
            }
        }
    }

    // DTOs para as requisições
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