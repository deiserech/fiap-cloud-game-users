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

                modelBuilder.Entity<Library>()
                    .HasOne(l => l.User)
                    .WithMany(u => u.LibraryGames)
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<Library>()
                    .HasOne(l => l.Game)
                    .WithMany(g => g.LibraryEntries)
                    .HasForeignKey(l => l.GameId)
                    .OnDelete(DeleteBehavior.Restrict); 

                modelBuilder.Entity<Library>()
                    .HasIndex(l => new { l.UserId, l.GameId })
                    .IsUnique();
            }
        }
    }
