using Lyra.Models;
using Microsoft.EntityFrameworkCore;

namespace Lyra.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<CareerPath> Career_Paths { get; set; } = null!;
    }
}
