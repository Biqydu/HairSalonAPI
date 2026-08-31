using HairSalon.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HairSalon.Api.Data.Configurations;

public sealed class BarberAvailabilityConfiguration : IEntityTypeConfiguration<BarberAvailability>
{
    public void Configure(EntityTypeBuilder<BarberAvailability> builder)
    {
        builder.HasOne(ba => ba.Barber)
            .WithMany()
            .HasForeignKey(ba => ba.BarberId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(a => new { a.BarberId, a.DayOfWeek })
            .IsUnique();
    }
}