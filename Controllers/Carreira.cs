using Lyra.Data;
using Lyra.DTOs;
using Lyra.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lyra.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CarreiraController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CarreiraController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ConcluirTrilha([FromBody] ConcluirTrilhaDto dto)
        {
            if (dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.Trilha))
                return BadRequest(new { message = "Dados inválidos." });

            var registro = new TrilhaConcluida
            {
                Trilha = dto.Trilha,
                Descricao = dto.Descricao,
                UserId = dto.UserId,
                DataConclusao = DateTime.UtcNow,
            };

            _context.Add(registro);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Trilha salva com sucesso!", trilha = registro.Trilha });
        }
    }
}
