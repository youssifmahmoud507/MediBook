using MediBook.Application.Common;
using MediBook.Application.DTOs.Patients;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class PatientService(IPatientRepository patientRepository) : IPatientService
    {
        private readonly IPatientRepository _patientRepository = patientRepository;

        public async Task<Result<Guid>> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
        {
            // Validate the request (basic if checks — non-empty required fields, DateOfBirth not in the future).
            if (request == null 
                || request.FirstName == null 
                || request.LastName == null 
                || request.PhoneNumber == null) {
                return  Result<Guid>.Failure("Invalid request. Required fields are missing.", ErrorType.Validation);
            }

            if (request.DateOfBirth > DateOnly.FromDateTime(DateTime.Today)) {
                return  Result<Guid>.Failure("Invalid request. Date of Birth cannot be in the future.", ErrorType.Validation);
            }
            var patient = new Patient
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true,
            };
            //patient.SetName(request.FirstName, request.LastName);
            await _patientRepository.CreatePatientAsync(patient, cancellationToken);
            return Result<Guid>.Success(patient.Id);
        }
    }
}
