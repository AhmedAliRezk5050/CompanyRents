using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class RenewalEntityTypeConfiguration : IEntityTypeConfiguration<Renewal>
{
    public void Configure(EntityTypeBuilder<Renewal> builder)
    {
        builder.HasIndex(x => x.AgreementNumber).IsUnique();

        builder.Property(entry => entry.Amount).HasPrecision(17, 2);
        builder.Property(entry => entry.TaxRatio).HasPrecision(17, 2);
        builder.Property(entry => entry.TotalAmount).HasPrecision(17, 2);

        builder
            .Property(i => i.TotalAmount)
            .HasComputedColumnSql("[Amount] + ([Amount] * [TaxRatio])");
        

        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.Renewal)
            .HasForeignKey(x => x.RenewalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.User)
            .WithMany(x => x.Renewals)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}