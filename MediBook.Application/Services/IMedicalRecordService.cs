using MediBook.Application.Common;
using MediBook.Application.DTOs.MedidcalRecords;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface IMedicalRecordService
    {
        Task<Result<Guid>> CreateMedicalRecordAsync(Guid appointmentId, CreateMedicalRecordRequest request, Guid currentUserId, CancellationToken cancellationToken);
        Task<Result<MedicalRecordResponse>> GetMedicalRecordAsync(Guid appointmentId, Guid currentUserId, CancellationToken cancellationToken);
    }
}
