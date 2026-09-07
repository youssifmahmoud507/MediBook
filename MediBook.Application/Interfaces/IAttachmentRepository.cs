using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Interfaces
{
    public interface IAttachmentRepository
    {
        Task AddAsync(Attachment attachment, CancellationToken cancellationToken);
        Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Attachment>> GetByMedicalRecordIdAsync(Guid medicalRecordId, CancellationToken cancellationToken);
    }
}
