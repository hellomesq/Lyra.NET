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

        // -------------------------------------------------------------
        //  POST - CREATE USER
        // -------------------------------------------------------------

        /// <summary>
        /// Cria um novo usuário no sistema.
        /// </summary>
        /// <param name="user">Dados do usuário a ser criado.</param>
        /// <returns>Retorna o usuário criado com links HATEOAS.</returns>
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
                new
                {
                    message = "Usuário cadastrado com sucesso!",
                    user = new
                    {
                        id = userId,
                        user.Name,
                        user.Email,
                        user.Experience_Level,
                        _links = new
                        {
                            self = Url.Action(nameof(GetUserById), new { id = userId }),
                            update = Url.Action(nameof(UpdateUser), new { id = userId }),
                            delete = Url.Action(nameof(DeleteUser), new { id = userId }),
                        },
                    },
                }
            );
        }

        // -------------------------------------------------------------
        //  GET USER BY EMAIL
        // -------------------------------------------------------------

        /// <summary>
        /// Retorna um usuário pelo email.
        /// </summary>
        [HttpGet("by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email é obrigatório." });

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            return Ok(
                new
                {
                    user.User_Id,
                    user.Name,
                    user.Email,
                    user.Experience_Level,
                    _links = new
                    {
                        self = Url.Action(nameof(GetUserById), new { id = user.User_Id }),
                        update = Url.Action(nameof(UpdateUser), new { id = user.User_Id }),
                        delete = Url.Action(nameof(DeleteUser), new { id = user.User_Id }),
                    },
                }
            );
        }

        // -------------------------------------------------------------
        //  GET ALL USERS WITH PAGINATION
        // -------------------------------------------------------------

        /// <summary>
        /// Retorna todos os usuários com paginação.
        /// </summary>
        /// <param name="page">Número da página.</param>
        /// <param name="pageSize">Quantidade de itens por página.</param>
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
                    _links = new
                    {
                        self = Url.Action(nameof(GetUserById), new { id = u.User_Id }),
                        update = Url.Action(nameof(UpdateUser), new { id = u.User_Id }),
                        delete = Url.Action(nameof(DeleteUser), new { id = u.User_Id }),
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
                    data = users,
                    _links = new
                    {
                        self = Url.Action(nameof(GetAllUsers), new { page, pageSize }),
                        next = page < totalPages
                            ? Url.Action(nameof(GetAllUsers), new { page = page + 1, pageSize })
                            : null,
                        prev = page > 1
                            ? Url.Action(nameof(GetAllUsers), new { page = page - 1, pageSize })
                            : null,
                    },
                }
            );
        }

        // -------------------------------------------------------------
        //  GET USER BY ID
        // -------------------------------------------------------------

        /// <summary>
        /// Retorna um usuário pelo identificador único.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            return Ok(
                new
                {
                    user.User_Id,
                    user.Name,
                    user.Email,
                    user.Experience_Level,
                    _links = new
                    {
                        self = Url.Action(nameof(GetUserById), new { id }),
                        update = Url.Action(nameof(UpdateUser), new { id }),
                        delete = Url.Action(nameof(DeleteUser), new { id }),
                    },
                }
            );
        }

        // -------------------------------------------------------------
        //  PUT - UPDATE USER
        // -------------------------------------------------------------

        /// <summary>
        /// Atualiza dados de um usuário existente.
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
            if (user == null)
                return NotFound(new { message = "Usuário não encontrado." });

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;
            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = dto.Password;
            if (!string.IsNullOrWhiteSpace(dto.Experience_Level))
                user.Experience_Level = dto.Experience_Level;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Usuário atualizado: {UserId}", id);

            return NoContent();
        }

        // -------------------------------------------------------------
        //  DELETE USER
        // -------------------------------------------------------------

        /// <summary>
        /// Remove um usuário e suas trilhas associadas.
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
