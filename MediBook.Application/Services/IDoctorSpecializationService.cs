using MediBook.Application.Common;
using MediBook.Application.DTOs.DoctorSpecializations;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IDoctorSpecializationService
    {
        Task<Result> AssignSpecialtyToDoctorAsync(Guid doctorId, AssignSpecialtyToDoctorRequest request, CancellationToken cancellationToken);
    }
}
