using Microsoft.EntityFrameworkCore;
using FiapCloudGames.Users.Domain.Entities;

namespace FiapCloudGames.Users.Infrastructure.Data
{
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
            public DbSet<User> Users { get; set; }
            public DbSet<Game> Games { get; set; }
            public DbSet<Library> Libraries { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}