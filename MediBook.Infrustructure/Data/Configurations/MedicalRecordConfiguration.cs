using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
    {
        public void Configure(EntityTypeBuilder<MedicalRecord> builder)
        {
            builder.ToTable("MedicalRecords");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Diagnosis).IsRequired().HasMaxLength(500);
            builder.Property(x => x.Notes).HasMaxLength(2000);
            builder.Property(x => x.TreatmentPlan).HasMaxLength(1000);
            builder.Property(x => x.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            builder.HasIndex(x => x.AppointmentId).IsUnique();
            builder.HasOne<Appointment>().WithMany().HasForeignKey(x => x.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
