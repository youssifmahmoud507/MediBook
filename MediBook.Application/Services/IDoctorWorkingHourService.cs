using MediBook.Application.Common;
using MediBook.Application.DTOs.DoctorWorkingHours;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services;

public interface IDoctorWorkingHourService
{
    Task<Result> AddWorkingHourAsync(Guid doctorId, AddWorkingHourRequest request, CancellationToken cancellationToken);
}