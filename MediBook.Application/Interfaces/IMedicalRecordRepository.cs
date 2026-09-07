using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IMedicalRecordRepository
    {
        Task AddAsync(MedicalRecord record, CancellationToken cancellationToken);
        Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken);
        Task<MedicalRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
