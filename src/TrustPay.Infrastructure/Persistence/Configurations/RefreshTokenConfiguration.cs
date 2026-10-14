using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TrustPay.Domain.Entities;

namespace TrustPay.Infrastructure.Persistence.Configurations
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
            public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasIndex(r=>r.TokenHash);
        }
              
    }
}


