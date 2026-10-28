using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xpress.Domain.Entities;

namespace Xpress.Infrastructure.Persistence.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Reference).IsRequired().HasMaxLength(100);
        builder.Property(t => t.CustomerId).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Amount).HasPrecision(18, 4);

        builder.HasIndex(t => t.Reference).IsUnique();

        builder.HasIndex(t => new { t.CustomerId, t.CreatedAt, t.Id });

        builder.HasIndex(t => t.LoanId);

        builder.HasOne<Loan>()
               .WithMany()
               .HasForeignKey(t => t.LoanId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
