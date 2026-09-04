using MediBook.Application.Common;
using MediBook.Application.DTOs.Doctors;
using MediBook.Application.DTOs.DoctorWorkingHours;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class DoctorService(IDoctorRepository doctorRepository , IDoctorWorkingHourRepository doctorWorkingHourRepository) : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository = doctorRepository;
        private readonly IDoctorWorkingHourRepository _doctorWorkingHourRepository = doctorWorkingHourRepository;
        public async Task<Result<Guid>> AddDoctorAsync(DoctorRequest request, CancellationToken cancellationToken)
        {
            if (request == null
                || request.FirstName == null
                || request.LastName == null
                || request.LicenseNumber == null
                || request.PhoneNumber == null
                || request.Email == null)
                return Result<Guid>.Failure("Invalid doctor request" , ErrorType.Validation);
            var licenseNumberExists = await _doctorRepository.ExistsWithLicenseNumberAsync(request.LicenseNumber, cancellationToken);
            if (licenseNumberExists)
                return Result<Guid>.Failure("A doctor with the same license number already exists.", ErrorType.Conflict);
            var doctor = new Doctor
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                LicenseNumber = request.LicenseNumber,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
            await _doctorRepository.AddAsync(doctor, cancellationToken);
            return Result<Guid>.Success(doctor.Id);
        }
        public async Task<Result<DoctorResponse>> GetDoctorByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id, cancellationToken);
            if (doctor == null)
                return Result<DoctorResponse>.Failure("Doctor not found", ErrorType.NotFound);
            return Result<DoctorResponse>.Success(new DoctorResponse
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                LicenseNumber = doctor.LicenseNumber,
                PhoneNumber = doctor.PhoneNumber,
                Email = doctor.Email
            });
        }

        public async Task<Result<DoctorWithWorkingHoursResponse>> GetDoctorWithWorkingHoursAsync(Guid id, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id ,cancellationToken);
            if (doctor == null)
                return Result<DoctorWithWorkingHoursResponse>.Failure("Doctor not found", ErrorType.NotFound);


            var workingHours = await _doctorWorkingHourRepository.GetByDoctorAsync(doctor.Id, cancellationToken);

            return Result<DoctorWithWorkingHoursResponse>.Success(new DoctorWithWorkingHoursResponse
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                LicenseNumber = doctor.LicenseNumber,
                PhoneNumber = doctor.PhoneNumber,
                Email = doctor.Email,
                WorkingHours = [.. workingHours.Select(wh => new WorkingHourResponse
                {
                    DayOfWeek = wh.DayOfWeek,
                    StartTime = wh.StartTime,
                    EndTime = wh.EndTime
                })]
            });
        }
    }
}
