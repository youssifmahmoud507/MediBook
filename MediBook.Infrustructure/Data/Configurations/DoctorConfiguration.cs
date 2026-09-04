using MediBook.Domain.Entities;
using MediBook.Infrustructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
    {
        public void Configure(EntityTypeBuilder<Doctor> builder)
        {
            builder.ToTable("Doctors");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(50);
            builder.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(20);
            builder.Property(x => x.Email).HasMaxLength(100).IsRequired();
            builder.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(20);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x=> x.LicenseNumber).IsUnique();
            builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(p => p.UserId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
