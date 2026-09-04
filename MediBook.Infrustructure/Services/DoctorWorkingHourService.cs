using MediBook.Application.Common;
using MediBook.Application.DTOs.DoctorWorkingHours;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class DoctorWorkingHourService(IDoctorWorkingHourRepository doctorWorkingHourRepository , IDoctorRepository doctorRepository) : IDoctorWorkingHourService
    {
        private readonly IDoctorWorkingHourRepository _doctorWorkingHourRepository = doctorWorkingHourRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;

        public async Task<Result> AddWorkingHourAsync(Guid doctorId, AddWorkingHourRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null)
                return Result.Failure("Doctor not found", ErrorType.NotFound);
            
            if (request.StartTime >= request.EndTime)
            {
                return Result.Failure("Start time must be before end time.", ErrorType.Validation);
            }

            var doctorWorkings = await _doctorWorkingHourRepository.GetByDoctorAndDayAsync(doctorId, request.DayOfWeek, cancellationToken);

            bool hasOverlap =  doctorWorkings.Any(existing => request.StartTime < existing.EndTime && existing.StartTime < request.EndTime);

            if (hasOverlap)
                return Result.Failure("This time range overlaps with an existing working hour.", ErrorType.Conflict);
            
            var newWorkingHour = new DoctorWorkingHour
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsActive = true
            };

            await _doctorWorkingHourRepository.AddAsync(newWorkingHour, cancellationToken);

            return Result.Success();
        }
    }
}
