using MediBook.Application.Common;
using MediBook.Application.DTOs.ClinicLocations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IClinicLocationService 
    {
        Task<Result<Guid>> AddAsync(ClinicLocationRequest request, CancellationToken cancellationToken);
        Task<Result<ClinicLocationResponse>> GetByIdResponseAsync(Guid id, CancellationToken cancellationToken);
    }
}
