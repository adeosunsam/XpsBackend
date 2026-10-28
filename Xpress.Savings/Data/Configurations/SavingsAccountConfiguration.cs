using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xpress.Savings.Domain;

namespace Xpress.Savings.Data.Configurations;

internal sealed class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
{
    public void Configure(EntityTypeBuilder<SavingsAccount> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.AccountNumber).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Status).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Balance).HasPrecision(18, 4);
        builder.HasIndex(s => s.AccountNumber).IsUnique();
        builder.HasIndex(s => s.LoanId).IsUnique();
        builder.HasIndex(s => s.CustomerId);
    }
}
