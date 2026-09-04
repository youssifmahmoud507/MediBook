using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IAppointmentRepository
    {
        Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
        Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateOnly date, CancellationToken cancellationToken);
        Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken);
    }
}
