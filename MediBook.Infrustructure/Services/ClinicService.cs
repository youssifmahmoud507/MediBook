using MediBook.Application.Common;
using MediBook.Application.DTOs.Clinics;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class ClinicService(IClinicRepository clinicRepository) : IClinicService
    {
        private readonly IClinicRepository _clinicRepository = clinicRepository;

        public async Task<Result<Guid>> AddAsync(ClinicRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name)
                || string.IsNullOrWhiteSpace(request.PhoneNumber)
                || string.IsNullOrWhiteSpace(request.Email))
            {
                return Result<Guid>.Failure("Name, PhoneNumber and Email are required fields." , ErrorType.Validation);
            }

            var CheckExistingEmail = await _clinicRepository.ExistsWithEmailAsync(request.Email, cancellationToken);
            if (CheckExistingEmail)
            {
                return Result<Guid>.Failure("A clinic with the same email already exists.", ErrorType.Conflict);
            }

            var clinic = new Clinic
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
            await _clinicRepository.AddAsync(clinic, cancellationToken);
            return Result<Guid>.Success(clinic.Id);

        }

        public async Task<Result<ClinicResponse>> GetByIdResponseAsync(Guid id, CancellationToken cancellationToken)
        {
            var clinic = await _clinicRepository.GetByIdAsync(id, cancellationToken);
            if (clinic == null)
            {
                return Result<ClinicResponse>.Failure("Clinic not found.", ErrorType.NotFound);
            }

            return Result<ClinicResponse>.Success(new ClinicResponse
            {
                Id = id,
                Name = clinic.Name,
                Description = clinic.Description,
                PhoneNumber = clinic.PhoneNumber,
                Email = clinic.Email
            });
        }
    }
}
