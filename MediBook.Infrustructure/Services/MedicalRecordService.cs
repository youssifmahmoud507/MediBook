using MediBook.Application.Common;
using MediBook.Application.DTOs.MedidcalRecords;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class MedicalRecordService(IMedicalRecordRepository medicalRecordRepository, IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository, IPatientRepository patientRepository) : IMedicalRecordService
    {
        private readonly IMedicalRecordRepository _medicalRecordRepository = medicalRecordRepository;
        private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;
        private readonly IPatientRepository _patientRepository = patientRepository;

        public async Task<Result<Guid>> CreateMedicalRecordAsync(Guid appointmentId, CreateMedicalRecordRequest request, Guid currentUserId, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment is null)
                return Result<Guid>.Failure("Appointment not found.", ErrorType.NotFound);

            var doctor = await _doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);
            if (doctor is null || doctor.UserId != currentUserId)
                return Result<Guid>.Failure("Doctor not found or unauthorized.", ErrorType.NotFound);

            if (string.IsNullOrWhiteSpace(request.Diagnosis))
                return Result<Guid>.Failure("Diagnosis is required.", ErrorType.Validation);

            var medicalRecord = new MedicalRecord
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                Diagnosis = request.Diagnosis,
                Notes = request.Notes,
                TreatmentPlan = request.TreatmentPlan,
                CreatedAt = DateTimeOffset.UtcNow
            };

            appointment.TryComplete();
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            await _medicalRecordRepository.AddAsync(medicalRecord, cancellationToken);
            return Result<Guid>.Success(medicalRecord.Id);
        }
        public async Task<Result<MedicalRecordResponse>> GetMedicalRecordAsync(Guid appointmentId, Guid currentUserId, CancellationToken cancellationToken)
        {
            var appointmentResult = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointmentResult is null)
                return Result<MedicalRecordResponse>.Failure("Appointment not found.", ErrorType.NotFound);

            var medicalRecordResult = await _medicalRecordRepository.GetByAppointmentIdAsync(appointmentId, cancellationToken);
            if (medicalRecordResult is null)
                return Result<MedicalRecordResponse>.Failure("Medical record not found.", ErrorType.NotFound);

            var doctorResult = await _doctorRepository.GetByIdAsync(appointmentResult.DoctorId, cancellationToken);
            if (doctorResult is null || doctorResult.UserId != currentUserId)
                return Result<MedicalRecordResponse>.Failure("Doctor not found or unauthorized.", ErrorType.NotFound);

            var patientResult = await _patientRepository.GetPatientByIdAsync(appointmentResult.PatientId, cancellationToken);
            if (patientResult is null)
                return Result<MedicalRecordResponse>.Failure("Patient not found.", ErrorType.NotFound);

            // Check: does currentUserId match patient.UserId OR doctor.UserId? If neither, return failure.
            if (patientResult.UserId != currentUserId && doctorResult.UserId != currentUserId)
                return Result<MedicalRecordResponse>.Failure("Unauthorized access to medical record.", ErrorType.NotFound);

            // If authorized, map to MedicalRecordResponse, return Result<MedicalRecordResponse>.Success(...).
            var response = new MedicalRecordResponse
            {
                Id = medicalRecordResult.Id,
                AppointmentId = medicalRecordResult.AppointmentId,
                Diagnosis = medicalRecordResult.Diagnosis,
                Notes = medicalRecordResult.Notes,
                TreatmentPlan = medicalRecordResult.TreatmentPlan,
                CreatedAt = medicalRecordResult.CreatedAt
            };
            return Result<MedicalRecordResponse>.Success(response);

        }
    }
}
