using MediBook.Application.Common;
using MediBook.Application.DTOs.Patients;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IPatientService
    {
        Task<Result<Guid>> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default);
    }
}
