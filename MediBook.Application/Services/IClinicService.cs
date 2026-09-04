using MediBook.Application.Common;
using MediBook.Application.DTOs.Clinics;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IClinicService
    {
        Task<Result<Guid>> AddAsync(ClinicRequest request, CancellationToken cancellationToken);
        Task<Result<ClinicResponse>> GetByIdResponseAsync(Guid id, CancellationToken cancellationToken);
    }
}
