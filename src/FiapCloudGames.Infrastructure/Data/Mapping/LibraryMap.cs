using FiapCloudGames.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.Users.Infrastructure.Data.Mapping
{
    public class LibraryMap : IEntityTypeConfiguration<Library>
    {
        public void Configure(EntityTypeBuilder<Library> builder)
        {
            builder.HasKey(l => l.Id);
            builder.Property(l => l.Id)
                .ValueGeneratedOnAdd();

            builder.Property(l => l.UserId)
                .IsRequired();

            builder.Property(l => l.GameId)
                .IsRequired();

            builder.Property(l => l.PurchaseId)
                .IsRequired();

            builder.HasOne(l => l.User)
                .WithMany(u => u.LibraryGames)
                .HasForeignKey(l => l.UserId);

            builder.HasOne(l => l.Game)
                .WithMany(g => g.LibraryEntries)
                .HasForeignKey(l => l.GameId);
        }
    }
}
