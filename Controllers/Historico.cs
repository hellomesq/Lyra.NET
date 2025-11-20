using Lyra.DTOs;
using Lyra.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lyra.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class HistoricoController : ControllerBase
    {
        private readonly HistoricoService _service;

        public HistoricoController(HistoricoService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var history = await _service.GetUserCareerHistory(userId);

            if (history == null || !history.Any())
                return NotFound(new { message = "Nenhuma trilha encontrada para este usuário." });

            return Ok(history);
        }
    }
}
