using Lyra.Data;
using Lyra.DTOs;
using Lyra.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyra.Services
{
    public class HistoricoService
    {
        private readonly AppDbContext _context;

        public HistoricoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<HistoricoDto>> GetUserCareerHistory(int userId)
        {
            var paths = await _context
                .Users.Where(u => u.User_Id == userId)
                .SelectMany(u => _context.Career_Paths.Where(p => p.User_Id == u.User_Id))
                .OrderByDescending(p => p.Created_At)
                .ToListAsync();

            return paths
                .Select(p => new HistoricoDto
                {
                    PathId = p.Path_Id,
                    Title = p.Title ?? string.Empty, // garante que não seja null
                    CreatedAt = p.Created_At ?? DateTime.MinValue,
                })
                .ToList();
        }
    }
}
