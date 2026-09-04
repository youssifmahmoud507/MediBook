using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Domain.Enums;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class AppointmentRepository(MediBookDbContext context) : IAppointmentRepository
    {
        private readonly MediBookDbContext _context = context;
        public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
        {
            await _context.Appointments.AddAsync(appointment, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Appointment>> GetByDoctorAndDateAsync(Guid doctorId, DateOnly date, CancellationToken cancellationToken)
        {
            var startOfDay = date.ToDateTime(TimeOnly.MinValue);
            var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

            return await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                            && a.Status != AppointmentStatus.Cancelled
                            && a.StartTime.Date >= startOfDay
                            && a.StartTime.Date <= endOfDay).ToListAsync(cancellationToken);
        }

        public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Appointments.FindAsync([id], cancellationToken);
        }
        public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
