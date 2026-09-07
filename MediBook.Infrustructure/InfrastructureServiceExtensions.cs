using MediBook.Application.Common;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Infrustructure.BackgroundJobs;
using MediBook.Infrustructure.Common;
using MediBook.Infrustructure.Data;
using MediBook.Infrustructure.Identity;
using MediBook.Infrustructure.Identity.implementation;
using MediBook.Infrustructure.Identity.Interfaces;
using MediBook.Infrustructure.Repositories;
using MediBook.Infrustructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<MediBookDbContext>(options =>options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
            });


            services.AddScoped<IPatientRepository, PatientRepository>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<IDoctorRepository, DoctorRepository>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IClinicLocationRepository, ClinicLocationRepository>();
            services.AddScoped<IClinicLocationService, ClinicLocationService>();
            services.AddScoped<IClinicRepository, ClinicRepository>();
            services.AddScoped<IClinicService, ClinicService>();
            services.AddScoped<ISpecialtyRepository, SpecialtyRepository>();
            services.AddScoped<ISpecialtyService, SpecialtyService>();
            services.AddScoped<IDoctorSpecializationRepository, DoctorSpecializationRepository>();
            services.AddScoped<IDoctorSpecializationService, DoctorSpecializationService>();
            services.AddScoped<IDoctorClinicAssignmentRepository, DoctorClinicAssignmentRepository>();
            services.AddScoped<IDoctorClinicAssignmentService, DoctorClinicAssignmentService>();
            services.AddScoped<IDoctorWorkingHourRepository, DoctorWorkingHourRepository>();
            services.AddScoped<IDoctorWorkingHourService, DoctorWorkingHourService>();
            services.AddScoped<IAppointmentTypeRepository, AppointmentTypeRepository>();
            services.AddScoped<IAppointmentTypeService, AppointmentTypeService>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IAppointmentService, AppointmentService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEmailSender, FakeEmailSender>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            services.AddHostedService<AppointmentCompletionBackgroundService>();

            return services;
        }
    }
}
