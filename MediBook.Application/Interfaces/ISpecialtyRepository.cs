using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface ISpecialtyRepository
    {
        Task AddAsync(Specialty specialty, CancellationToken cancellationToken);
        Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<bool> ExistNameAsync(string name, CancellationToken cancellationToken);
    }
}
