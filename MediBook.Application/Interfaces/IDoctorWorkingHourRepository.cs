using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IDoctorWorkingHourRepository
    {
        Task AddAsync(DoctorWorkingHour workingHour, CancellationToken cancellationToken);
        Task<List<DoctorWorkingHour>> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek dayOfWeek, CancellationToken cancellationToken);
        Task<List<DoctorWorkingHour>> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken);
    }
}
