using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class ClinicLocationConfiguration : IEntityTypeConfiguration<ClinicLocation>
    {
        public void Configure(EntityTypeBuilder<ClinicLocation> builder)
        {
            builder.ToTable("ClinicLocations");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
            builder.Property(x => x.AddressLine).IsRequired().HasMaxLength(100);
            builder.Property(x => x.City).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Country).IsRequired().HasMaxLength(50);
            builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(50);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.ClinicId).IsRequired();
            builder.HasOne(x => x.Clinic).WithMany().HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
