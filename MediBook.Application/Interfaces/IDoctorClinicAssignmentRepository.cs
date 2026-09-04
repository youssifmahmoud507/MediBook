using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IDoctorClinicAssignmentRepository
    {
        Task AddAsync(DoctorClinicAssignment assignment, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid doctorId, Guid clinicLocationId, CancellationToken cancellationToken);
    }
}
