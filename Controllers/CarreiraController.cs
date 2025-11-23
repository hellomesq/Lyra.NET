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

        // ====================================================================
        //  GET – LISTAR TRILHAS POR USUÁRIO
        // ====================================================================

        /// <summary>
        /// Retorna todas as trilhas concluídas por um usuário com paginação.
        /// </summary>
        [HttpGet("{userId}")]
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
                    t.Id,
                    t.Trilha,
                    t.Descricao,
                    t.DataConclusao,
                    _links = new
                    {
                        self = Url.Action(
                            nameof(GetTrilhasDoUsuario),
                            new
                            {
                                userId,
                                page,
                                pageSize,
                            }
                        ),
                        update = Url.Action(nameof(UpdateTrilha), new { id = t.Id }),
                        delete = Url.Action(nameof(DeleteTrilha), new { id = t.Id }),
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
                    data = trilhas,
                    _links = new
                    {
                        self = Url.Action(
                            nameof(GetTrilhasDoUsuario),
                            new
                            {
                                userId,
                                page,
                                pageSize,
                            }
                        ),
                        next = page < totalPages
                            ? Url.Action(
                                nameof(GetTrilhasDoUsuario),
                                new
                                {
                                    userId,
                                    page = page + 1,
                                    pageSize,
                                }
                            )
                            : null,
                        prev = page > 1
                            ? Url.Action(
                                nameof(GetTrilhasDoUsuario),
                                new
                                {
                                    userId,
                                    page = page - 1,
                                    pageSize,
                                }
                            )
                            : null,
                    },
                }
            );
        }

        // ====================================================================
        //  POST – CRIAR TRILHA
        // ====================================================================

        /// <summary>
        /// Registra a conclusão de uma nova trilha.
        /// </summary>
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

            _context.Career_Paths.Add(registro);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetTrilhasDoUsuario),
                new { userId = dto.UserId },
                new
                {
                    message = "Trilha registrada com sucesso!",
                    trilha = new
                    {
                        registro.Id,
                        registro.Trilha,
                        registro.Descricao,
                        registro.DataConclusao,
                        _links = new
                        {
                            self = Url.Action(
                                nameof(GetTrilhasDoUsuario),
                                new { userId = dto.UserId }
                            ),
                            update = Url.Action(nameof(UpdateTrilha), new { id = registro.Id }),
                            delete = Url.Action(nameof(DeleteTrilha), new { id = registro.Id }),
                        },
                    },
                }
            );
        }

        // ====================================================================
        //  PUT – ATUALIZAR TRILHA
        // ====================================================================

        /// <summary>
        /// Atualiza uma trilha concluída.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrilha(int id, [FromBody] CarreiraDto dto)
        {
            var trilha = await _context.Career_Paths.FindAsync(id);

            if (trilha == null)
                return NotFound(new { message = "Trilha não encontrada." });

            trilha.Trilha = dto.Trilha ?? trilha.Trilha;
            trilha.Descricao = dto.Descricao ?? trilha.Descricao;

            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    message = "Trilha atualizada com sucesso!",
                    trilha = new
                    {
                        trilha.Id,
                        trilha.Trilha,
                        trilha.Descricao,
                        trilha.DataConclusao,
                        _links = new
                        {
                            self = Url.Action(nameof(UpdateTrilha), new { id }),
                            delete = Url.Action(nameof(DeleteTrilha), new { id }),
                            list = Url.Action(
                                nameof(GetTrilhasDoUsuario),
                                new { userId = trilha.UserId }
                            ),
                        },
                    },
                }
            );
        }

        // ====================================================================
        //  DELETE – APAGAR TRILHA
        // ====================================================================

        /// <summary>
        /// Remove uma trilha concluída.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrilha(int id)
        {
            var trilha = await _context.Career_Paths.FindAsync(id);

            if (trilha == null)
                return NotFound(new { message = "Trilha não encontrada." });

            _context.Remove(trilha);
            await _context.SaveChangesAsync();

            return Ok(
                new
                {
                    message = "Trilha removida com sucesso!",
                    _links = new
                    {
                        list = Url.Action(
                            nameof(GetTrilhasDoUsuario),
                            new { userId = trilha.UserId }
                        ),
                        create = Url.Action(nameof(ConcluirTrilha)),
                    },
                }
            );
        }
    }
}
