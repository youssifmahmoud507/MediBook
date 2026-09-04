using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class DoctorClinicAssignmentConfiguration : IEntityTypeConfiguration<DoctorClinicAssignment>
    {
        public void Configure(EntityTypeBuilder<DoctorClinicAssignment> builder)
        { 
            builder.ToTable("DoctorClinicAssignments");
            builder.HasKey(dca => new { dca.DoctorId, dca.ClinicLocationId });
            builder.HasOne(dca => dca.Doctor).WithMany().HasForeignKey(dca => dca.DoctorId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(dca => dca.ClinicLocation).WithMany().HasForeignKey(dca => dca.ClinicLocationId).OnDelete(DeleteBehavior.Cascade);
            builder.Property(dca => dca.IsActive).IsRequired();
        }
    }
}
