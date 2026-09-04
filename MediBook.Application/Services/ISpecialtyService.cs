using MediBook.Application.Common;
using MediBook.Application.DTOs.Specialities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface ISpecialtyService
    {
        Task<Result<Guid>> AddSpecialtyAsync(SpecialityRequest request, CancellationToken cancellationToken);
        Task<Result<SpecialityResponse>> GetSpecialtyAsync(Guid id , CancellationToken cancellationToken);
    }
}
