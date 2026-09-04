using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class ClinicRepository(MediBookDbContext context) : IClinicRepository
    {
        private readonly MediBookDbContext _context = context;
        public async Task AddAsync(Clinic clinic, CancellationToken ct)
        {
            await _context.Clinics.AddAsync(clinic, ct);
            await _context.SaveChangesAsync(ct);
        }
        public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct) => await _context.Clinics.AnyAsync(x => x.Email == email, ct);
        public async Task<Clinic?> GetByIdAsync(Guid id, CancellationToken ct) => await _context.Clinics.FindAsync([id], ct);
    }
}
