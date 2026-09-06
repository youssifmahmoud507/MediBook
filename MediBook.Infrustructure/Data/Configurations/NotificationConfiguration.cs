using MediBook.Domain.Entities;
using MediBook.Infrustructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data.Configurations
{
    internal class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.Property(n => n.Title).IsRequired().HasMaxLength(100);
            builder.Property(n => n.Message).IsRequired().HasMaxLength(500);
            builder.Property(n => n.UserId).IsRequired();
            builder.Property(n => n.CreatedAt).IsRequired();
            builder.Property(n => n.IsRead).IsRequired();
            builder.HasKey(n => n.Id);
            builder.HasIndex(n => n.UserId);
            builder.Property(n => n.CreatedAt).IsRequired().HasColumnType("datetimeoffset");
            builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
