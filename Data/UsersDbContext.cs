using Microsoft.EntityFrameworkCore;
using FiapCloudGames.Users.Models;

namespace FiapCloudGames.Users.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<LibraryEntry> Library { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<LibraryEntry>().HasKey(l => l.Id);
        base.OnModelCreating(modelBuilder);
    }
}
