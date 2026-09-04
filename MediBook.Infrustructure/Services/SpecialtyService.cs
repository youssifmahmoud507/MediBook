using MediBook.Application.Common;
using MediBook.Application.DTOs.Specialities;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class SpecialtyService(ISpecialtyRepository specialtyRepository) : ISpecialtyService
    {
        private readonly ISpecialtyRepository _specialtyRepository = specialtyRepository;

        public async Task<Result<Guid>> AddSpecialtyAsync(SpecialityRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<Guid>.Failure("Specialty name is required.", ErrorType.Validation);

            var nameExists = await _specialtyRepository.ExistNameAsync(request.Name, cancellationToken);
            if (nameExists)
                return Result<Guid>.Failure("A specialty with the same name already exists.", ErrorType.Conflict);

            var specialty = new Specialty
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsActive = true
            };
            await _specialtyRepository.AddAsync(specialty, cancellationToken);
            return Result<Guid>.Success(specialty.Id);

        }

        public async Task<Result<SpecialityResponse>> GetSpecialtyAsync(Guid id, CancellationToken cancellationToken)
        {
            var specialty = await _specialtyRepository.GetByIdAsync(id, cancellationToken);
            if (specialty == null)
                return Result<SpecialityResponse>.Failure("Specialty not found.", ErrorType.NotFound);
            var response = new SpecialityResponse
            {
                Id = specialty.Id,
                Name = specialty.Name,
                Description = specialty.Description,
                IsActive = specialty.IsActive
            };
            return Result<SpecialityResponse>.Success(response);
        }
    }
}
