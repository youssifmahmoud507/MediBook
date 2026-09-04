using MediBook.Application.Common;
using MediBook.Application.DTOs.ClinicLocations;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class ClinicLocationService(IClinicLocationRepository clinicLocationRepository, IClinicRepository clinicRepository) : IClinicLocationService
    {
        private readonly IClinicLocationRepository _clinicLocationRepository = clinicLocationRepository;
        private readonly IClinicRepository _clinicRepository = clinicRepository;

        public async Task<Result<Guid>> AddAsync(ClinicLocationRequest request, CancellationToken cancellationToken)
        {
            var clinic = await _clinicRepository.GetByIdAsync(request.ClinicId, cancellationToken);
            if (clinic == null) return Result<Guid>.Failure("Clinic not found" , ErrorType.NotFound);

            if (string.IsNullOrWhiteSpace(request.Name)
                || string.IsNullOrWhiteSpace(request.PhoneNumber)
                || string.IsNullOrWhiteSpace(request.TimeZoneId)
                || string.IsNullOrWhiteSpace(request.AddressLine)
                || string.IsNullOrWhiteSpace(request.City)
                || string.IsNullOrWhiteSpace(request.Country)) 
                return Result<Guid>.Failure("Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.", ErrorType.Validation);

            var clinicLocation = new ClinicLocation
            {
                Id = Guid.NewGuid(),
                ClinicId = request.ClinicId,
                Name = request.Name,
                AddressLine = request.AddressLine,
                City = request.City,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                TimeZoneId = request.TimeZoneId,
                IsActive = true
            };
            await _clinicLocationRepository.AddAsync(clinicLocation, cancellationToken);
            return Result<Guid>.Success(clinicLocation.Id);
        }

        public async Task<Result<ClinicLocationResponse>> GetByIdResponseAsync(Guid id, CancellationToken cancellationToken)
        {
            var clinicLocation = await _clinicLocationRepository.GetByIdAsync(id, cancellationToken);
            if (clinicLocation == null) return Result<ClinicLocationResponse>.Failure("Clinic location not found.", ErrorType.NotFound);

            return Result<ClinicLocationResponse>.Success(new ClinicLocationResponse
            {
                ClinicId = clinicLocation.ClinicId,
                ClinicLocationId = clinicLocation.Id,
                Name = clinicLocation.Name,
                AddressLine = clinicLocation.AddressLine,
                City = clinicLocation.City,
                Country = clinicLocation.Country,
                PhoneNumber = clinicLocation.PhoneNumber,
                TimeZoneId = clinicLocation.TimeZoneId,
            });
        }
    }
}
