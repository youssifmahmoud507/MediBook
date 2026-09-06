using MediBook.Domain.Entities;
using MediBook.Infrustructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Data
{
    public class MediBookDbContext(DbContextOptions<MediBookDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MediBookDbContext).Assembly);
        }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentType> AppointmentTypes { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<ClinicLocation> ClinicLocations { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorClinicAssignment> DoctorClinicAssignments { get; set; }
        public DbSet<DoctorSpecialization> DoctorSpecializations { get; set; }
        public DbSet<DoctorWorkingHour> DoctorWorkingHours { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
    }
}
