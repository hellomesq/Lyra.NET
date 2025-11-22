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

    // Criar usuário
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserDto user)
    {
        var userId = await _service.InserirUsuario(
            user.Name,
            user.Email,
            user.Password,
            user.Experience_Level
        );

        return Created("", new { id = userId, message = "Usuário cadastrado com sucesso!" });
    }

    // Listar todos os usuários
    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    // Pegar usuário por ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(user);
    }

    // Atualizar usuário
    [HttpPut("{id}")]
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
        return NoContent();
    }

    // Deletar usuário
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == id);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        // Deletar trilhas concluídas
        var trilhas = _context.Career_Paths.Where(t => t.UserId == id);
        _context.Career_Paths.RemoveRange(trilhas);

        await _context.SaveChangesAsync();

        // Deletar usuário
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Pegar usuário por email
    [HttpGet("by-email")]
    public async Task<IActionResult> GetUserByEmail([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "Email é obrigatório." });

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return NotFound(new { message = "Usuário não encontrado." });

        return Ok(user);
    }
}
