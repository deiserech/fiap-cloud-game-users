using FiapCloudGames.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Users.Infrastructure.Data.Mapping
{
    public class GameMap : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id)
                .ValueGeneratedOnAdd();

            builder.Property(g => g.Code)
                .IsRequired();

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(g => g.UpdatedAt)
                .IsRequired();

            builder.Property(g => g.IsActive)
                .IsRequired();

            builder.Property(g => g.RemovedAt);

            builder.HasMany(g => g.LibraryEntries)
                .WithOne(l => l.Game)
                .HasForeignKey(l => l.GameId);

            builder.HasIndex(g => g.Code)
                .IsUnique();
        }
    }
}
