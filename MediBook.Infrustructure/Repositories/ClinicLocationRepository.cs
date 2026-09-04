using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class ClinicLocationRepository(MediBookDbContext context) : IClinicLocationRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(ClinicLocation location, CancellationToken cancellationToken)
        {
            await _context.ClinicLocations.AddAsync(location, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<ClinicLocation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.ClinicLocations.FindAsync([id], cancellationToken);
        }
    }
}
