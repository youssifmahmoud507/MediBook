using MediBook.Application.Common;
using MediBook.Application.DTOs.Doctors;
using MediBook.Application.DTOs.DoctorWorkingHours;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IDoctorService
    {
        Task<Result<Guid>> AddDoctorAsync(DoctorRequest request, CancellationToken cancellationToken);
        Task<Result<DoctorResponse>> GetDoctorByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<DoctorWithWorkingHoursResponse>> GetDoctorWithWorkingHoursAsync(Guid id, CancellationToken cancellationToken);

    }
}
