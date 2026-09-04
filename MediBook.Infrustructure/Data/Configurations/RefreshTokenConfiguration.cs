using MediBook.Domain.Entities;
using MediBook.Infrustructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Token).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ExpiresAt).IsRequired().HasColumnType("datetimeoffset");
            builder.Property(x => x.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            builder.Property(x => x.IsRevoked).IsRequired();
            builder.HasIndex(x => x.Token).IsUnique();
            builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
