using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class MedicalRecordRepository(MediBookDbContext context) : IMedicalRecordRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(MedicalRecord record, CancellationToken cancellationToken)
        {
            await _context.MedicalRecords.AddAsync(record, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<MedicalRecord?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken)
        {
            return await _context.MedicalRecords.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId, cancellationToken);
        }
    }
}
