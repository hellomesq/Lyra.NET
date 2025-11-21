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

        // Retorna o histórico de trilhas de um usuário
        public async Task<List<HistoricoDto>> GetUserCareerHistory(int userId)
        {
            var paths = await _context
                .Career_Paths.Where(p => p.User_Id == userId)
                .OrderByDescending(p => p.Created_At)
                .ToListAsync();

            return paths
                .Select(p => new HistoricoDto
                {
                    PathId = p.Path_Id,
                    Title = p.Title ?? string.Empty,
                    CreatedAt = p.Created_At ?? DateTime.MinValue,
                    User_Id = p.User_Id,
                })
                .ToList();
        }

        // Insere um histórico de trilha para um usuário
        public async Task InserirHistorico(HistoricoDto dto)
        {
            var careerPath = new CareerPath
            {
                Title = dto.Title,
                Created_At = dto.CreatedAt,
                User_Id = dto.User_Id,
            };

            _context.Career_Paths.Add(careerPath);
            await _context.SaveChangesAsync();
        }
    }
}
