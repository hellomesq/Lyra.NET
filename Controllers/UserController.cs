using Lyra.Data;
using Lyra.DTOs;
using Lyra.Models;
using Lyra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lyra.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _service;
        private readonly AppDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            UserService service,
            AppDbContext context,
            ILogger<UserController> logger
        )
        {
            _service = service;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] UserDto user)
        {
            if (string.IsNullOrWhiteSpace(user.Name) || string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new { message = "Nome e Email são obrigatórios." });

            var userId = await _service.InserirUsuario(
                user.Name,
                user.Email,
                user.Password,
                user.Experience_Level
            );
            _logger.LogInformation("Usuário criado: {Email} (ID: {UserId})", user.Email, userId);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = userId },
                new { id = userId, message = "Usuário cadastrado com sucesso!" }
            );
        }

        /// <summary>
        /// Lista todos os usuários com paginação.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            if (page <= 0)
                page = 1;
            if (pageSize <= 0)
                pageSize = 10;

            var totalUsers = await _context.Users.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = await _context
                .Users.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.User_Id,
                    u.Name,
                    u.Email,
                    u.Experience_Level,
                    links = new[]
                    {
                        new
                        {
                            rel = "self",
                            href = Url.Action(nameof(GetUserById), new { id = u.User_Id }),
                        },
                        new
                        {
                            rel = "update",
                            href = Url.Action(nameof(UpdateUser), new { id = u.User_Id }),
                        },
                        new
                        {
                            rel = "delete",
                            href = Url.Action(nameof(DeleteUser), new { id = u.User_Id }),
                        },
                    },
                })
                .ToListAsync();

            return Ok(
                new
                {
                    page,
                    pageSize,
                    totalUsers,
                    totalPages,
                    users,
                }
            );
        }

        /// <summary>
        /// Retorna usuário pelo ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            return Ok(user);
        }

        /// <summary>
        /// Atualiza usuário existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.Password = dto.Password;
            user.Experience_Level = dto.Experience_Level;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuário atualizado: {UserId}", id);

            return NoContent();
        }

        /// <summary>
        /// Deleta usuário e suas trilhas.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            var trilhas = _context.Career_Paths.Where(t => t.UserId == id);
            _context.Career_Paths.RemoveRange(trilhas);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuário deletado: {UserId}", id);
            return NoContent();
        }
    }
}
