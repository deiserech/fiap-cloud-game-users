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

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasMany(g => g.LibraryEntries)
                .WithOne(l => l.Game)
                .HasForeignKey(l => l.GameId);
        }
    }
}
