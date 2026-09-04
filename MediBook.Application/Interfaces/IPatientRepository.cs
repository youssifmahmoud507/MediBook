using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task CreatePatientAsync(Patient patient , CancellationToken cancellationToken = default);
        Task<Patient?> GetPatientByIdAsync(Guid patientId , CancellationToken cancellationToken = default);
    }
}
