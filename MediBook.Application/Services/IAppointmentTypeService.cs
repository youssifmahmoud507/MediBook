using MediBook.Application.Common;
using MediBook.Application.DTOs.AppointmentTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IAppointmentTypeService
    {
        Task<Result<Guid>> CreateAppointmentTypeAsync(CreateAppointmentTypeRequest request, CancellationToken cancellationToken);
        Task<Result<AppointmentTypeResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
