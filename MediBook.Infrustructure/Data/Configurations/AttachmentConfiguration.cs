using MediBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class AttachmentConfiguration : IEntityTypeConfiguration<Domain.Entities.Attachment>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.Attachment> builder)
        {
            builder.ToTable("Attachments");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            builder.Property(x => x.StoredFileName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.ContentType).IsRequired().HasMaxLength(100);
            builder.Property(x => x.UploadedAt).IsRequired().HasColumnType("datetimeoffset");
            builder.HasIndex(x => x.MedicalRecordId);
            builder.HasOne<MedicalRecord>().WithMany().HasForeignKey(x => x.MedicalRecordId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
