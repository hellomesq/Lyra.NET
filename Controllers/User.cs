using Lyra.Data;
using Lyra.DTOs;
using Lyra.Models;
using Lyra.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _service;
    private readonly AppDbContext _context;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService service, AppDbContext context, ILogger<UserController> logger)
    {
        _service = service;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] UserDto user)
    {
        await _service.InserirUsuario(user.Name, user.Email, user.Password, user.Experience_Level);
        return Created("", new { message = "Usuário cadastrado com sucesso via PROCEDURE!" });
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        ApiVersion? apiVersion = null
    )
    {
        var total = await _context.Users.CountAsync();
        var users = await _context
            .Users.OrderBy(u => u.User_Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        string version = apiVersion?.ToString() ?? "1.0";
        string url = $"{Request.Scheme}://{Request.Host}/api/v{version}/User";

        var response = new
        {
            page,
            pageSize,
            total,
            data = users,
            links = new
            {
                self = $"{url}?page={page}&pageSize={pageSize}",
                next = page * pageSize < total
                    ? $"{url}?page={page + 1}&pageSize={pageSize}"
                    : null,
                prev = page > 1 ? $"{url}?page={page - 1}&pageSize={pageSize}" : null,
            },
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        var links = new
        {
            self = Url.Action(nameof(GetUserById), new { id }),
            update = Url.Action(nameof(UpdateUser), new { id }),
            delete = Url.Action(nameof(DeleteUser), new { id }),
        };

        return Ok(new { user, links });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Password = dto.Password;
        user.Experience_Level = dto.Experience_Level;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
