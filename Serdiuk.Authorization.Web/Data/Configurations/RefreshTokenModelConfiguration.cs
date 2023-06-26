﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Serdiuk.Authorization.Web.Data.IdentityModels;

namespace Serdiuk.Authorization.Web.Data.Configurations
{
    public class RefreshTokenModelConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id);
            builder.Property(x => x.UserId);
            builder.Property(x => x.ExpiresAt);
            builder.Property(x => x.IsRevoked);
            builder.Property(x => x.Token);
        }
    }
}
