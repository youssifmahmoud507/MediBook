using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IDoctorSpecializationRepository
    {
        Task AddAsync(DoctorSpecialization doctorSpecialization, CancellationToken cancellationToken);
        Task<bool> ExistsAsync(Guid doctorId, Guid specialtyId, CancellationToken cancellationToken);
    }
}
