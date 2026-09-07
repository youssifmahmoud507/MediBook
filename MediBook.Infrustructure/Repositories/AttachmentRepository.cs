using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class AttachmentRepository(MediBookDbContext context) : IAttachmentRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(Attachment attachment, CancellationToken cancellationToken)
        {
            await _context.Attachments.AddAsync(attachment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Attachments.FindAsync([id], cancellationToken);
        }

        public async Task<List<Attachment>> GetByMedicalRecordIdAsync(Guid medicalRecordId, CancellationToken cancellationToken)
        {
            return await _context.Attachments.Where(a => a.MedicalRecordId == medicalRecordId).ToListAsync(cancellationToken);
        }
    }
}
