using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xpress.Domain.Entities;

namespace Xpress.Infrastructure.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Status).IsRequired().HasMaxLength(50);
        builder.Property(l => l.Amount).HasPrecision(18, 4);

        builder.HasIndex(l => l.CustomerId);
    }
}
