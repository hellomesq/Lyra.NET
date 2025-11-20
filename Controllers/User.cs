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
    public async Task<IActionResult> CreateUser([FromBody] UserDto user)
    {
        await _service.InserirUsuario(user.Name, user.Email, user.Password, user.Experience_Level);
        return Created("", new { message = "Usuário cadastrado com sucesso via PROCEDURE!" });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users.ToListAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        // Pega email vindo do token
        var emailFromFirebase = HttpContext.Items["FirebaseEmail"]?.ToString();
        if (emailFromFirebase == null)
            return Unauthorized(new { message = "Token inválido ou ausente." });

        // Busca o usuário do banco baseado no email do Firebase
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailFromFirebase);

        if (user == null)
            return Forbid("Usuário autenticado no Firebase, mas não existe no banco.");

        // Bloqueia se tentar acessar outro ID
        if (user.User_Id != id)
            return Forbid("Você não tem permissão para acessar informações de outro usuário.");

        return Ok(new { user });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UserDto dto)
    {
        var emailFromFirebase = HttpContext.Items["FirebaseEmail"]?.ToString();
        if (emailFromFirebase == null)
            return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailFromFirebase);

        if (user == null)
            return Forbid();

        if (user.User_Id != id)
            return Forbid();

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Password = dto.Password;
        user.Experience_Level = dto.Experience_Level;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var emailFromFirebase = HttpContext.Items["FirebaseEmail"]?.ToString();
        if (emailFromFirebase == null)
            return Unauthorized();

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailFromFirebase);

        if (user == null)
            return Forbid();

        if (user.User_Id != id)
            return Forbid();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckUser()
    {
        // Email vindo do token do Firebase (middleware)
        var emailFromFirebase = HttpContext.Items["FirebaseEmail"]?.ToString();

        if (emailFromFirebase == null)
            return Unauthorized(new { message = "Token inválido ou ausente." });

        // Verifica se existe no BD
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailFromFirebase);

        if (user == null)
            return NotFound(
                new
                {
                    message = "Usuário existe no Firebase, mas não existe no banco.",
                    email = emailFromFirebase,
                }
            );

        return Ok(new { message = "Usuário validado com sucesso.", user });
    }
}
