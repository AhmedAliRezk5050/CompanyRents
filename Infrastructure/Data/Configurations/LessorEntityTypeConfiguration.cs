using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class LessorEntityTypeConfiguration : IEntityTypeConfiguration<Lessor>
{
    public void Configure(EntityTypeBuilder<Lessor> builder)
    {
        builder.HasIndex(x => x.ContractNumber).IsUnique();
        
        builder.Property(entry => entry.RentAmount).HasPrecision(17, 2);
        builder.Property(entry => entry.RentTaxRatio).HasPrecision(17, 2);
        builder.Property(entry => entry.TotalRentAmount).HasPrecision(17, 2);
        
        
        builder
            .Property(i => i.TotalRentAmount)
            .HasComputedColumnSql("[RentAmount] + ([RentAmount] * [RentTaxRatio])");
        
        builder.HasMany(x => x.ParticipationRatios)
            .WithOne(x => x.Lessor)
            .HasForeignKey(x => x.LessorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.Lessor)
            .HasForeignKey(x => x.LessorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(x => x.Renewals)
            .WithOne(x => x.Lessor)
            .HasForeignKey(x => x.LessorId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        
        builder.HasOne(c => c.User)  
            .WithMany(x => x.Lessors)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}