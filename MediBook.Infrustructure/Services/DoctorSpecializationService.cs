using MediBook.Application.Common;
using MediBook.Application.DTOs.DoctorSpecializations;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class DoctorSpecializationService(IDoctorSpecializationRepository doctorSpecializationRepository, ISpecialtyRepository specialtyRepository, IDoctorRepository doctorRepository) : IDoctorSpecializationService
    {
        private readonly IDoctorSpecializationRepository _doctorSpecializationRepository = doctorSpecializationRepository;
        private readonly ISpecialtyRepository _specialtyRepository = specialtyRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;

        public async Task<Result> AssignSpecialtyToDoctorAsync(Guid doctorId, AssignSpecialtyToDoctorRequest request, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null)
                return Result.Failure("Doctor not found", ErrorType.NotFound);

            var specialty = await _specialtyRepository.GetByIdAsync(request.SpecialtyId, cancellationToken);
            if (specialty == null)
                return Result.Failure("Specialty not found", ErrorType.NotFound);

            var alreadyAssigned = await _doctorSpecializationRepository.ExistsAsync(doctorId, request.SpecialtyId, cancellationToken);
            if (alreadyAssigned)
                return Result.Failure("This specialty is already assigned to this doctor.", ErrorType.Conflict);

            var doctorSpecialization = new DoctorSpecialization
            {
                DoctorId = doctorId,
                SpecializationId = request.SpecialtyId,
                IsPrimary = request.IsPrimary
            };

            await _doctorSpecializationRepository.AddAsync(doctorSpecialization, cancellationToken);
            return Result.Success();
        }
    }
}
