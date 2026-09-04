using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class AppointmentTypeRepository(MediBookDbContext context) : IAppointmentTypeRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(AppointmentType appointmentType, CancellationToken cancellationToken)
        {
            await _context.AppointmentTypes.AddAsync(appointmentType, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<AppointmentType?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.AppointmentTypes.FindAsync([id], cancellationToken);
        }
    }
}
