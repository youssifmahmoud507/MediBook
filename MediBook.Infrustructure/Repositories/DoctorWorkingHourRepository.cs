using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class DoctorWorkingHourRepository(MediBookDbContext dbContext) : IDoctorWorkingHourRepository
    {
        private readonly MediBookDbContext _dbContext = dbContext;

        public async Task AddAsync(DoctorWorkingHour workingHour, CancellationToken cancellationToken)
        {
            await _dbContext.DoctorWorkingHours.AddAsync(workingHour, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<DoctorWorkingHour>> GetByDoctorAndDayAsync(Guid doctorId, DayOfWeek dayOfWeek, CancellationToken cancellationToken) => await _dbContext.DoctorWorkingHours.Where(x => x.DoctorId == doctorId && x.DayOfWeek == dayOfWeek).ToListAsync(cancellationToken);
        public async Task<List<DoctorWorkingHour>> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken) => await _dbContext.DoctorWorkingHours.Where(x => x.DoctorId == doctorId).ToListAsync(cancellationToken);

    }
}
