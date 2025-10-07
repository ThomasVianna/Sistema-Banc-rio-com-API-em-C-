using Microsoft.AspNetCore.Mvc;
using ProjetoBanco.Models;
using ProjetoBanco.Services;

namespace ProjetoBanco.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BancoController : ControllerBase
    {
        private readonly BancoService _bancoService;

        public BancoController(BancoService bancoService)
        {
            _bancoService = bancoService;
        }

        [HttpGet("clientes")]
        public IActionResult GetClientes() => Ok(_bancoService.GetClientes());

        [HttpGet("clientes/{id}")]
        public IActionResult GetCliente(int id)
        {
            var cliente = _bancoService.GetCliente(id);
            if (cliente == null) return NotFound();
            return Ok(cliente);
        }

        [HttpPost("clientes")]
        public IActionResult CriarCliente([FromBody] Cliente cliente)
        {
            var novo = _bancoService.CriarCliente(cliente);
            return CreatedAtAction(nameof(GetCliente), new { id = novo.Id }, novo);
        }

        [HttpPost("deposito/{id}")]
        public IActionResult Depositar(int id, [FromQuery] decimal valor)
        {
            return _bancoService.Depositar(id, valor)
                ? Ok("Depósito realizado.")
                : BadRequest("Erro no depósito.");
        }

        [HttpPost("saque/{id}")]
        public IActionResult Sacar(int id, [FromQuery] decimal valor)
        {
            return _bancoService.Sacar(id, valor)
                ? Ok("Saque realizado.")
                : BadRequest("Saldo insuficiente ou valor inválido.");
        }

        [HttpPost("transferir")]
        public IActionResult Transferir([FromQuery] int origemId, [FromQuery] int destinoId, [FromQuery] decimal valor)
        {
            return _bancoService.Transferir(origemId, destinoId, valor)
                ? Ok("Transferência concluída.")
                : BadRequest("Erro na transferência.");
        }
    }
}
