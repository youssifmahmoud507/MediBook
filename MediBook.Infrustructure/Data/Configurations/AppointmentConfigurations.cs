using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class AppointmentConfigurations : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ToTable("Appointments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.StartTime).IsRequired().HasColumnType("datetimeoffset");
            builder.Property(x => x.EndTime).IsRequired().HasColumnType("datetimeoffset");
            builder.Property(x => x.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            builder.Property(x => x.CancelledAt).HasColumnType("datetimeoffset");
            builder.Property(x => x.Status).IsRequired();
            builder.HasOne(x => x.Patient).WithMany().HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Doctor).WithMany().HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.AppointmentType).WithMany().HasForeignKey(x => x.AppointmentTypeId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ClinicLocation).WithMany().HasForeignKey(x => x.ClinicLocationId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
