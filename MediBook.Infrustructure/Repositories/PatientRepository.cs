using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class PatientRepository(MediBookDbContext context) : IPatientRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task CreatePatientAsync(Patient patient, CancellationToken cancellationToken = default)
        {
             await _context.Patients.AddAsync(patient , cancellationToken);
             await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<Patient?> GetPatientByIdAsync(Guid patientId, CancellationToken cancellationToken = default)
        {
            return await _context.Patients.FindAsync([patientId], cancellationToken);
        }
    }
}
