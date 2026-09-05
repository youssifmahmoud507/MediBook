using MediBook.Application.Common;
using MediBook.Application.DTOs.Appointments;
using MediBook.Application.DTOs.SchedulingAndSlot;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using MediBook.Domain.Enums;
using MediBook.Infrustructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Data;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace MediBook.Infrustructure.Services
{
    public class AppointmentService(IAppointmentRepository appointmentRepository,
                                    IDoctorRepository doctorRepository,
                                    IPatientRepository patientRepository,
                                    IClinicLocationRepository clinicLocationRepository,
                                    IAppointmentTypeRepository appointmentTypeRepository,
                                    IDoctorClinicAssignmentRepository doctorClinicAssignmentRepository,
                                    IDoctorWorkingHourRepository doctorWorkingHourRepository,
                                    IUnitOfWork unitOfWork,
                                    IDistributedCache cache) : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;
        private readonly IPatientRepository _patientRepository = patientRepository;
        private readonly IClinicLocationRepository _clinicLocationRepository = clinicLocationRepository;
        private readonly IAppointmentTypeRepository _appointmentTypeRepository = appointmentTypeRepository;
        private readonly IDoctorClinicAssignmentRepository _doctorClinicAssignmentRepository = doctorClinicAssignmentRepository;
        private readonly IDoctorWorkingHourRepository _doctorWorkingHourRepository = doctorWorkingHourRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IDistributedCache _distributedCache = cache;

        public async Task<Result<Guid>> CreateAppointmentAsync(CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            var doctorExists = await _doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
            if (doctorExists is null)
            {
                return Result<Guid>.Failure($"Doctor with ID {request.DoctorId} does not exist.", ErrorType.NotFound);
            }

            var patientExists = await _patientRepository.GetPatientByIdAsync(request.PatientId, cancellationToken);
            if (patientExists is null)
            {
                return Result<Guid>.Failure($"Patient with ID {request.PatientId} does not exist.", ErrorType.NotFound);
            }

            var clinicLocationExists = await _clinicLocationRepository.GetByIdAsync(request.ClinicLocationId, cancellationToken);
            if (clinicLocationExists is null)
            {
                return Result<Guid>.Failure($"ClinicLocation with ID {request.ClinicLocationId} does not exist.", ErrorType.NotFound);
            }

            var appointmentTypeExists = await _appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, cancellationToken);
            if (appointmentTypeExists is null)
            {
                return Result<Guid>.Failure($"AppointmentType with ID {request.AppointmentTypeId} does not exist.", ErrorType.NotFound);
            }

            var doctorClinicAssignmentExists = await _doctorClinicAssignmentRepository.ExistsAsync(request.DoctorId, request.ClinicLocationId, cancellationToken);
            if (!doctorClinicAssignmentExists)
            {
                return Result<Guid>.Failure($"Doctor with ID {request.DoctorId} is not assigned to ClinicLocation with ID {request.ClinicLocationId}.", ErrorType.NotFound);
            }

            if (request.StartTime < DateTimeOffset.UtcNow)
                return Result<Guid>.Failure("Cannot book an appointment in the past.", ErrorType.Validation);

            if (!doctorExists.IsActive)
                return Result<Guid>.Failure("Cannot book an appointment with an inactive doctor.", ErrorType.Validation);

            if (!appointmentTypeExists.IsActive)
                return Result<Guid>.Failure("Cannot book an appointment with an inactive appointment type.", ErrorType.Validation);


            var durationMinutes = appointmentTypeExists.DurationMinutes;
            var endTime = request.StartTime.AddMinutes(appointmentTypeExists.DurationMinutes);

            var clinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clinicLocationExists.TimeZoneId);

            var localStart = TimeZoneInfo.ConvertTime(request.StartTime,clinicTimeZone);

            var localEnd = TimeZoneInfo.ConvertTime(endTime,clinicTimeZone);

            if (localStart.Date != localEnd.Date)
            {
                return Result<Guid>.Failure("Appointment cannot span across different calendar days.", ErrorType.Validation);
            }

            var localDayOfWeek = localStart.DayOfWeek;

            var localStartTime = TimeOnly.FromDateTime(localStart.DateTime);
            var localEndTime = TimeOnly.FromDateTime(localEnd.DateTime);

            var workingHours = await _doctorWorkingHourRepository.GetByDoctorAndDayAsync( request.DoctorId, localDayOfWeek,cancellationToken);

            var isWithinWorkingHours = workingHours.Any(workingHour => workingHour.StartTime <= localStartTime && localEndTime <= workingHour.EndTime);

            if (!isWithinWorkingHours)
            {
                return Result<Guid>.Failure("The appointment time is outside the doctor's working hours.",ErrorType.Validation);
            }


            await _unitOfWork.BeginTransactionAsync(cancellationToken, IsolationLevel.Serializable);

            try
            {
                var existingAppointments = await _appointmentRepository.GetByDoctorAndDateAsync(
                    request.DoctorId, DateOnly.FromDateTime(localStart.DateTime), cancellationToken);

                var hasConflict = existingAppointments.Any(existing =>
                    request.StartTime < existing.EndTime && existing.StartTime < endTime);

                if (hasConflict)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<Guid>.Failure("The doctor already has an appointment during this time.", ErrorType.Conflict);
                }

                var appointment = new Appointment
                {
                    Id = Guid.NewGuid(),
                    DoctorId = request.DoctorId,
                    PatientId = request.PatientId,
                    ClinicLocationId = request.ClinicLocationId,
                    AppointmentTypeId = request.AppointmentTypeId,
                    StartTime = request.StartTime,
                    EndTime = endTime,
                    Status = AppointmentStatus.Confirmed,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                await _appointmentRepository.AddAsync(appointment, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result<Guid>.Success(appointment.Id);
            }
            catch (DbUpdateException)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Guid>.Failure("This time slot was just booked by someone else. Please try a different time.", ErrorType.Conflict);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        public async Task<Result<AppointmentResponse>> GetAppointmentByIdAsync(Guid id,CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id,cancellationToken);

            if (appointment is null)
            {
                return Result<AppointmentResponse>.Failure($"Appointment with ID {id} does not exist.",ErrorType.NotFound);
            }

            var response = new AppointmentResponse
            {
                Id = appointment.Id,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                ClinicLocationId = appointment.ClinicLocationId,
                AppointmentTypeId = appointment.AppointmentTypeId,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                CreatedAt = appointment.CreatedAt
            };

            return Result<AppointmentResponse>.Success(response);
        }
        public async Task<Result> CancelAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
            if (appointment is null)
                return Result.Failure("Appointment not found.", ErrorType.NotFound);

            if (appointment.Status == AppointmentStatus.Cancelled)
                return Result.Failure("Appointment is already cancelled.", ErrorType.Conflict);

            if (appointment.Status == AppointmentStatus.Completed)
                return Result.Failure("Cannot cancel a completed appointment.", ErrorType.Conflict);

            appointment.Cancel(DateTimeOffset.UtcNow);
            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            return Result.Success();
        }
        public async Task<Result<List<AvailableSlotResponse>>> GetAvailableSlotsAsync(Guid doctorId, GetAvailableSlotsRequest request, CancellationToken cancellationToken)
        {
            var cacheKey = $"available-slots:{doctorId}:{request.ClinicLocationId}:{request.AppointmentTypeId}:{request.Date:yyyy-MM-dd}";

            var cachedResult = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (cachedResult is not null)
            {
                var cachedSlots = JsonSerializer.Deserialize<List<AvailableSlotResponse>>(cachedResult)!;
                return Result<List<AvailableSlotResponse>>.Success(cachedSlots);
            }

            /*Existence checks: Doctor, ClinicLocation, AppointmentType (same pattern as Appointment creation — you know this cold now).*/
            var doctorExists = await _doctorRepository.GetByIdAsync(doctorId, cancellationToken);
            if (doctorExists is null)
            {
                return Result<List<AvailableSlotResponse>>.Failure($"Doctor with ID {doctorId} does not exist.", ErrorType.NotFound);
            }
            
            var clinicLocationExists = await _clinicLocationRepository.GetByIdAsync(request.ClinicLocationId, cancellationToken);
            if (clinicLocationExists is null)
            {
                return Result<List<AvailableSlotResponse>>.Failure($"ClinicLocation with ID {request.ClinicLocationId} does not exist.", ErrorType.NotFound);
            }
            
            var appointmentTypeExists = await _appointmentTypeRepository.GetByIdAsync(request.AppointmentTypeId, cancellationToken);
            if (appointmentTypeExists is null)
            {
                return Result<List<AvailableSlotResponse>>.Failure($"AppointmentType with ID {request.AppointmentTypeId} does not exist.", ErrorType.NotFound);
            }

            /*Doctor-ClinicLocation assignment check.*/
            var doctorClinicAssignmentExists = await _doctorClinicAssignmentRepository.ExistsAsync(doctorId, request.ClinicLocationId, cancellationToken);
            if (!doctorClinicAssignmentExists)
            {
                return Result<List<AvailableSlotResponse>>.Failure($"Doctor {doctorId} is not assigned to ClinicLocation {request.ClinicLocationId}.", ErrorType.NotFound);
            }

            if (!doctorExists.IsActive)
            {
                return Result<List<AvailableSlotResponse>>.Failure($"Doctor {doctorId} is not active.", ErrorType.Validation);
            }

            if (!appointmentTypeExists.IsActive)
            {
                return Result<List<AvailableSlotResponse>>.Failure($"AppointmentType with ID {request.AppointmentTypeId} is not active.", ErrorType.NotFound);
            }
            /*Determine the day-of-week from request.Date directly (request.Date.DayOfWeek) — no timezone conversion needed here, since you're working from a plain calendar date, not an instant. Note this is different from Appointment creation, where you converted an instant into local time; here you're starting from a local calendar date already.*/
            var dayOfWeek = request.Date.DayOfWeek;

            /*Fetch DoctorWorkingHour rows for that doctor/day via GetByDoctorAndDayAsync.*/
            var workingHours = await _doctorWorkingHourRepository.GetByDoctorAndDayAsync(doctorId, dayOfWeek, cancellationToken);

            /*For each working-hour block, generate candidate start times stepping by AppointmentType.DurationMinutes, from workingHour.StartTime up to (but not exceeding) workingHour.EndTime - duration. For each candidate local TimeOnly, combine with request.Date to build a local DateTime, then convert to a real DateTimeOffset using the clinic's timezone (TimeZoneInfo, same as before).*/
            var availableSlots = new List<AvailableSlotResponse>();
            var clinicTimeZone = TimeZoneInfo.FindSystemTimeZoneById(clinicLocationExists.TimeZoneId);
            var duration = appointmentTypeExists.DurationMinutes;

            foreach (var workingHour in workingHours)
            {
                var candidateTime = workingHour.StartTime.ToTimeSpan();
                var endTime = workingHour.EndTime.ToTimeSpan();
                var durationSpan = TimeSpan.FromMinutes(duration);

                while (candidateTime + durationSpan <= endTime)
                {
                    var candidateDateTime = new DateTime(
                        request.Date.Year, request.Date.Month, request.Date.Day,
                        candidateTime.Hours, candidateTime.Minutes, candidateTime.Seconds);

                    var offset = clinicTimeZone.GetUtcOffset(candidateDateTime);
                    var candidateStart = new DateTimeOffset(candidateDateTime, offset);
                    var candidateEnd = candidateStart.AddMinutes(duration);

                    availableSlots.Add(new AvailableSlotResponse
                    {
                        StartTime = candidateStart,
                        EndTime = candidateEnd
                    });

                    candidateTime += durationSpan;
                }
            }

            /*Fetch existing appointments for that doctor/date (GetByDoctorAndDateAsync).*/
            var existingAppointments = await _appointmentRepository.GetByDoctorAndDateAsync(doctorId, request.Date, cancellationToken);

            /*Filter out any candidate whose [start, start+duration) overlaps any existing appointment (reuse the overlap formula).*/
            availableSlots = [.. availableSlots.Where(slot => slot.StartTime > DateTimeOffset.UtcNow).Where(slot => !existingAppointments.Any(app => app.StartTime < slot.EndTime && app.EndTime > slot.StartTime))];

            var json = JsonSerializer.Serialize(availableSlots);
            await _distributedCache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            }, cancellationToken);

            return Result<List<AvailableSlotResponse>>.Success(availableSlots);
        }
    }
}
