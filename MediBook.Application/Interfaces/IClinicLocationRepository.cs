using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IClinicLocationRepository
    {
        Task AddAsync(ClinicLocation location, CancellationToken cancellationToken);
        Task<ClinicLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
