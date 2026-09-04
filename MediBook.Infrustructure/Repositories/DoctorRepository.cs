using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class DoctorRepository(MediBookDbContext context) : IDoctorRepository
    {
        private readonly MediBookDbContext _context = context;
        public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken)
        {
            await _context.Doctors.AddAsync(doctor, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> ExistsWithLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken)=> await _context.Doctors.AnyAsync(x => x.LicenseNumber == licenseNumber, cancellationToken);
        public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => await _context.Doctors.FindAsync([id], cancellationToken);
    }
}
