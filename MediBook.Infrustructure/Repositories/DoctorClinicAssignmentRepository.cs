using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class DoctorClinicAssignmentRepository(MediBookDbContext context) : IDoctorClinicAssignmentRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(DoctorClinicAssignment assignment, CancellationToken cancellationToken)
        {
            await _context.DoctorClinicAssignments.AddAsync(assignment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid doctorId, Guid clinicLocationId, CancellationToken cancellationToken)
        {
            return await _context.DoctorClinicAssignments.AnyAsync(x => x.DoctorId == doctorId && x.ClinicLocationId == clinicLocationId, cancellationToken);
        }
    }
}
