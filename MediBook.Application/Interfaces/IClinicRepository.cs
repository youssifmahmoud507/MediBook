using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IClinicRepository
    {
        Task AddAsync(Clinic clinic, CancellationToken ct);
        Task<Clinic?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct);
    }
}
