using MediBook.Application.Common;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Repositories;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class AttachmentService(IAttachmentRepository attachmentRepository , IAppointmentRepository appointmentRepository , IMedicalRecordRepository medicalRecordRepository , IDoctorRepository doctorRepository , IFileStorageService fileStorageService , IPatientRepository patientRepository) : IAttachmentService
    {
        private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
        private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;
        private readonly IMedicalRecordRepository _medicalRecordRepository = medicalRecordRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;
        private readonly IFileStorageService _fileStorageService = fileStorageService;
        private readonly IPatientRepository _patientRepository = patientRepository;

        public async Task<Result<(Stream FileStream, string ContentType, string FileName)>> DownloadAttachmentAsync(Guid attachmentId, Guid currentUserId, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(attachmentId, cancellationToken);
            if (attachment is null)
                return Result<(Stream, string, string)>.Failure("Attachment not found.", ErrorType.NotFound);

            var medicalRecord = await _medicalRecordRepository.GetByIdAsync(attachment.MedicalRecordId, cancellationToken);
            if (medicalRecord is null)
                return Result<(Stream, string, string)>.Failure("Medical record not found.", ErrorType.NotFound);

            var appointment = await _appointmentRepository.GetByIdAsync(medicalRecord.AppointmentId, cancellationToken);
            if (appointment is null)
                return Result<(Stream, string, string)>.Failure("Appointment not found.", ErrorType.NotFound);

            var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);
            var patient = await _patientRepository.GetPatientByIdAsync(appointment.PatientId, cancellationToken);

            var isDoctor = doctor?.UserId == currentUserId;
            var isPatient = patient?.UserId == currentUserId;

            if (!isDoctor && !isPatient)
                return Result<(Stream, string, string)>.Failure("Unauthorized access to attachment.", ErrorType.NotFound);

            var fileStream = await _fileStorageService.GetFileAsync(attachment.StoredFileName, cancellationToken);
            if (fileStream is null)
                return Result<(Stream, string, string)>.Failure("File not found in storage.", ErrorType.NotFound);

            return Result<(Stream, string, string)>.Success((fileStream, attachment.ContentType, attachment.FileName));
        }
        public async Task<Result<Guid>> UploadAttachmentAsync(Guid medicalRecordId, IFormFile file, Guid currentUserId, CancellationToken cancellationToken)
        {
            var medicalRecord = await _medicalRecordRepository.GetByIdAsync(medicalRecordId, cancellationToken);
            if (medicalRecord == null)
                return Result<Guid>.Failure("Medical record not found.", ErrorType.NotFound);

            var appointment = await _appointmentRepository.GetByIdAsync(medicalRecord.AppointmentId, cancellationToken);
            if (appointment == null)
                return Result<Guid>.Failure("Appointment not found.", ErrorType.NotFound);

            var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);
            if (doctor == null || doctor.UserId != currentUserId)
                return Result<Guid>.Failure("Doctor not found or unauthorized.", ErrorType.NotFound);

            var storedFileName = await _fileStorageService.SaveFileAsync(file.OpenReadStream(), file.FileName, cancellationToken);

            var attachment = new Attachment
            {
                Id = Guid.NewGuid(),
                MedicalRecordId = medicalRecordId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                StoredFileName = storedFileName,
                UploadedAt = DateTimeOffset.UtcNow,
            };

            await _attachmentRepository.AddAsync(attachment, cancellationToken);
            return Result<Guid>.Success(attachment.Id);
        }
    }
}
