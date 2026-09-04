using MediBook.Application.Common;
using MediBook.Application.DTOs.AppointmentTypes;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class AppointmentTypeService(IAppointmentTypeRepository repository) : IAppointmentTypeService
    {
        private readonly IAppointmentTypeRepository _repository = repository;
        public async Task<Result<Guid>> CreateAppointmentTypeAsync(CreateAppointmentTypeRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<Guid>.Failure("Name is required.", ErrorType.Validation);

            if (request.DurationMinutes <= 0)
                return Result<Guid>.Failure("Duration must be greater than zero.", ErrorType.Validation);

            var appointmentType = new AppointmentType
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                IsActive = true
            };

            await _repository.AddAsync(appointmentType, cancellationToken);
            return Result<Guid>.Success(appointmentType.Id);
        }
        public async Task<Result<AppointmentTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var appointmentType = await _repository.GetByIdAsync(id, cancellationToken);
            if (appointmentType == null)
                return Result<AppointmentTypeResponse>.Failure("Appointment type not found.", ErrorType.NotFound);

            return Result<AppointmentTypeResponse>.Success(new AppointmentTypeResponse
            {
                Id = appointmentType.Id,
                Name = appointmentType.Name,
                Description = appointmentType.Description,
                DurationMinutes = appointmentType.DurationMinutes,
                IsActive = appointmentType.IsActive
            });
        }
    }
}
