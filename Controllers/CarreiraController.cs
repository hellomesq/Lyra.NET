using Lyra.Data;
using Lyra.DTOs;
using Lyra.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetTrilhasDoUsuario(int userId)
        {
            var trilhas = await _context
                .Career_Paths.Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.Trilha,
                    t.Descricao,
                    t.DataConclusao,
                })
                .ToListAsync();

            if (!trilhas.Any())
                return NotFound(new { message = "Nenhuma trilha encontrada para esse usuário." });

            return Ok(trilhas);
        }

        [HttpPost]
        public async Task<IActionResult> ConcluirTrilha([FromBody] CarreiraDto dto)
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

            return Ok(
                new
                {
                    message = "Trilha salva com sucesso!",
                    trilha = registro.Trilha,
                    descricao = registro.Descricao,
                }
            );
        }
    }
}
