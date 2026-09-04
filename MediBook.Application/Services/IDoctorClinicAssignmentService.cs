using MediBook.Application.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IDoctorClinicAssignmentService
    {
        Task<Result> AssignDoctorToLocationAsync(Guid doctorId, Guid clinicLocationId, CancellationToken cancellationToken);
    }
}
