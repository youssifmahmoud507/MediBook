using MediBook.Application.Common;
using MediBook.Application.DTOs.Appointments;
using MediBook.Application.DTOs.SchedulingAndSlot;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IAppointmentService
    {
        Task<Result<Guid>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken);
        Task<Result<AppointmentResponse>> GetAppointmentByIdAsync(Guid id,CancellationToken cancellationToken);
        Task<Result> CancelAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken);
        Task<Result<List<AvailableSlotResponse>>> GetAvailableSlotsAsync(Guid doctorId,GetAvailableSlotsRequest request,CancellationToken cancellationToken);
    }
}
