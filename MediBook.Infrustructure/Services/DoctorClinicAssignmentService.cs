using MediBook.Application.Common;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class DoctorClinicAssignmentService(IDoctorClinicAssignmentRepository assignmentRepository,IDoctorRepository doctorRepository,IClinicLocationRepository clinicLocationRepository) : IDoctorClinicAssignmentService
    {
        private readonly IDoctorClinicAssignmentRepository _assignmentRepository = assignmentRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;
        private readonly IClinicLocationRepository _clinicLocationRepository = clinicLocationRepository;
        public async Task<Result> AssignDoctorToLocationAsync(Guid doctorId, Guid clinicLocationId, CancellationToken cancellationToken)
        {
            var doctor = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
            if (doctor == null)
                return Result.Failure("Doctor not found", ErrorType.NotFound);

            var clinicLocation = await _clinicLocationRepository.GetByIdAsync(clinicLocationId, cancellationToken);
            if (clinicLocation == null)
                return Result.Failure("Clinic location not found", ErrorType.NotFound);

            var alreadyAssigned = await _assignmentRepository.ExistsAsync(doctorId, clinicLocationId, cancellationToken);
            if (alreadyAssigned)
                return Result.Failure("This doctor is already assigned to this clinic location.", ErrorType.Conflict);

            var assignment = new DoctorClinicAssignment
            {
                DoctorId = doctorId,
                ClinicLocationId = clinicLocationId,
                IsActive = true
            };

            await _assignmentRepository.AddAsync(assignment, cancellationToken);
            return Result.Success();
        }
    }
}
