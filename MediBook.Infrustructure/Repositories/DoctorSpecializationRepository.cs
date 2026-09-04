using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class DoctorSpecializationRepository(MediBookDbContext context) : IDoctorSpecializationRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(DoctorSpecialization doctorSpecialization, CancellationToken cancellationToken)
        {
            await _context.DoctorSpecializations.AddAsync(doctorSpecialization, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid doctorId, Guid specialtyId, CancellationToken cancellationToken)
        {
            return await _context.DoctorSpecializations.AnyAsync(x => x.DoctorId == doctorId && x.SpecializationId == specialtyId, cancellationToken);
        }
    }
}
