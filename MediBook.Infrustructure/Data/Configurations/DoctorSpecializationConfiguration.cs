using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class DoctorSpecializationConfiguration : IEntityTypeConfiguration<DoctorSpecialization>
    {
        public void Configure(EntityTypeBuilder<DoctorSpecialization> builder)
        {
            builder.ToTable("DoctorSpecializations");
            builder.HasKey(ds => new { ds.DoctorId, ds.SpecializationId });
            builder.Property(ds => ds.IsPrimary).IsRequired();
            builder.HasOne(ds => ds.Doctor).WithMany().HasForeignKey(ds => ds.DoctorId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(ds => ds.Specialization).WithMany().HasForeignKey(ds => ds.SpecializationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
