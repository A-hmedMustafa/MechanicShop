using MechanicShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MechanicShop.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> refreshToken)
    {
        refreshToken.ToTable("RefreshTokens");

        refreshToken.HasKey(rt => rt.Id).IsClustered(false);

        refreshToken.Property(rt => rt.Token).HasMaxLength(200);

        refreshToken.HasIndex(rt => rt.Token).IsUnique();

        refreshToken.Property(rt => rt.UserId).IsRequired();

        refreshToken.Property(rt => rt.ExpiresAtUtc).IsRequired();
    }
}



