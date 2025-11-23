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
        private readonly ILogger<CarreiraController> _logger;

        public CarreiraController(AppDbContext context, ILogger<CarreiraController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTrilhasDoUsuario(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (userId <= 0)
                return BadRequest(new { message = "ID de usuário inválido." });

            if (page <= 0)
                page = 1;
            if (pageSize <= 0)
                pageSize = 10;

            var totalTrilhas = await _context.Career_Paths.CountAsync(t => t.UserId == userId);
            if (totalTrilhas == 0)
                return NotFound(new { message = "Nenhuma trilha encontrada para esse usuário." });

            var totalPages = (int)Math.Ceiling(totalTrilhas / (double)pageSize);

            var trilhas = await _context
                .Career_Paths.Where(t => t.UserId == userId)
                .OrderByDescending(t => t.DataConclusao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Trilha,
                    t.Descricao,
                    t.DataConclusao,
                    links = new[]
                    {
                        new
                        {
                            rel = "self",
                            href = Url.Action(
                                nameof(GetTrilhasDoUsuario),
                                new
                                {
                                    userId,
                                    page,
                                    pageSize,
                                }
                            ),
                        },
                        new { rel = "concluir", href = Url.Action(nameof(ConcluirTrilha)) },
                    },
                })
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalTrilhas,
                    totalPages,
                    trilhas,
                }
            );
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

            _context.Career_Paths.Add(registro);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Trilha concluída pelo usuário {UserId}", dto.UserId);

            return CreatedAtAction(
                nameof(GetTrilhasDoUsuario),
                new { userId = dto.UserId },
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
