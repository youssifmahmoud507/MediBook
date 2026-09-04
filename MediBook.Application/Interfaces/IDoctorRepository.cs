using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IDoctorRepository
    {
        Task AddAsync(Doctor doctor, CancellationToken cancellationToken);
        Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistsWithLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken);
    } 
}
