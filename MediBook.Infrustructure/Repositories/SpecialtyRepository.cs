using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class SpecialtyRepository(MediBookDbContext context) : ISpecialtyRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(Specialty specialty, CancellationToken cancellationToken)
        {
            await _context.Specialties.AddAsync(specialty, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistNameAsync(string name, CancellationToken cancellationToken)
        {
            return await _context.Specialties.AnyAsync(s => s.Name == name, cancellationToken);
        }

        public async Task<Specialty?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Specialties.FindAsync([id], cancellationToken);
        }
    }
}
