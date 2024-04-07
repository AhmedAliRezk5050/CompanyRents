using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class PaymentEntityTypeConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(entry => entry.PaidAmount).HasPrecision(17, 2);
        builder.Property(entry => entry.RemainingAmount).HasPrecision(17, 2);
        
        
        builder.HasOne(c => c.User)  
            .WithMany(x => x.Payments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}