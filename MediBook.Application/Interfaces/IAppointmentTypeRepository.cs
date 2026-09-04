using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IAppointmentTypeRepository
    {
        Task AddAsync(AppointmentType appointmentType, CancellationToken cancellationToken);
        Task<AppointmentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
