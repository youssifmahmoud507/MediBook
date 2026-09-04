using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class DoctorWorkingHourConfiguration : IEntityTypeConfiguration<DoctorWorkingHour>
    {
        public void Configure(EntityTypeBuilder<DoctorWorkingHour> builder)
        {
            builder.ToTable("DoctorWorkingHours");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.EndTime).IsRequired().HasColumnType("time");
            builder.Property(d => d.StartTime).IsRequired().HasColumnType("time");
            builder.Property(d => d.DayOfWeek).IsRequired().HasColumnType("int");
            builder.Property(d => d.DoctorId).IsRequired();
            builder.Property(d => d.IsActive).IsRequired();
            builder.HasOne(d => d.Doctor).WithMany().HasForeignKey(d => d.DoctorId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
