using MediBook.Application.Common;
using MediBook.Application.DTOs.Appointments;
using MediBook.Application.DTOs.SchedulingAndSlot;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using MediBook.Domain.Enums;
using MediBook.Infrustructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Data;

namespace MediBook.Test
{
    public class AppointmentServiceTests
    {
        private static CreateAppointmentRequest CreateRequest(DateTimeOffset? startTime = null)
        {
            return new CreateAppointmentRequest(
                PatientId: Guid.NewGuid(),
                DoctorId: Guid.NewGuid(),
                ClinicLocationId: Guid.NewGuid(),
                AppointmentTypeId: Guid.NewGuid(),
                StartTime: startTime ?? DateTimeOffset.UtcNow.AddDays(1));
        }

        private static Doctor CreateDoctor(bool isActive = true)
        {
            return new Doctor
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "01000000000",
                Email = "doctor@test.com",
                LicenseNumber = "LIC-001",
                IsActive = isActive,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static Patient CreatePatient(bool isActive = true)
        {
            return new Patient
            {
                Id = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Doe",
                DateOfBirth = new DateOnly(1995, 1, 1),
                PhoneNumber = "01100000000",
                Email = "patient@test.com",
                IsActive = isActive,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static ClinicLocation CreateClinicLocation(string timeZoneId = "UTC")
        {
            return new ClinicLocation
            {
                Id = Guid.NewGuid(),
                ClinicId = Guid.NewGuid(),
                Name = "Main Clinic",
                AddressLine = "123 Main Street",
                City = "Cairo",
                Country = "Egypt",
                PhoneNumber = "0220000000",
                TimeZoneId = timeZoneId,
                IsActive = true
            };
        }

        private static AppointmentType CreateAppointmentType(bool isActive = true, int durationMinutes = 30)
        {
            return new AppointmentType
            {
                Id = Guid.NewGuid(),
                Name = "General Consultation",
                Description = "General medical consultation",
                DurationMinutes = durationMinutes,
                IsActive = isActive
            };
        }

        private static DoctorWorkingHour CreateWorkingHour(Guid doctorId,DayOfWeek dayOfWeek,TimeOnly startTime,TimeOnly endTime)
        {
            return new DoctorWorkingHour
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = true
            };
        }

        private static Appointment CreateAppointment(Guid doctorId,Guid patientId,Guid clinicLocationId,Guid appointmentTypeId,DateTimeOffset startTime,DateTimeOffset endTime,AppointmentStatus status = AppointmentStatus.Confirmed)
        {
            return new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = doctorId,
                PatientId = patientId,
                ClinicLocationId = clinicLocationId,
                AppointmentTypeId = appointmentTypeId,
                StartTime = startTime,
                EndTime = endTime,
                Status = status,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static AppointmentService CreateSut(Mock<IAppointmentRepository> appointmentRepository,Mock<IDoctorRepository> doctorRepository,Mock<IPatientRepository> patientRepository,Mock<IClinicLocationRepository> clinicLocationRepository,Mock<IAppointmentTypeRepository> appointmentTypeRepository,Mock<IDoctorClinicAssignmentRepository> doctorClinicAssignmentRepository,Mock<IDoctorWorkingHourRepository> doctorWorkingHourRepository,Mock<IUnitOfWork> unitOfWork)
        {
            return new AppointmentService(
                appointmentRepository.Object,
                doctorRepository.Object,
                patientRepository.Object,
                clinicLocationRepository.Object,
                appointmentTypeRepository.Object,
                doctorClinicAssignmentRepository.Object,
                doctorWorkingHourRepository.Object,
                unitOfWork.Object);
        }

        private static void SetupUnitOfWork(
            Mock<IUnitOfWork> unitOfWork)
        {
            unitOfWork
                .Setup(u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>(),
                    It.IsAny<IsolationLevel>()))
                .Returns(Task.CompletedTask);

            unitOfWork
                .Setup(u => u.CommitTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            unitOfWork
                .Setup(u => u.RollbackTransactionAsync(
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task CreateAppointmentAsync_DoctorDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = CreateRequest();

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_PatientDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = CreateRequest();

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_ClinicLocationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicLocation?)null);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = CreateRequest();

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_AppointmentTypeDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentType?)null);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = CreateRequest();

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_DoctorNotAssignedToClinic_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                new DateTimeOffset(
                    2026, 9, 7, 10, 0, 0, TimeSpan.Zero));

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);

            unitOfWork.Verify(
                u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>(),
                    It.IsAny<IsolationLevel>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentAsync_StartTimeInPast_ReturnsValidation()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                DateTimeOffset.UtcNow.AddMinutes(-30));

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_DoctorInactive_ReturnsValidation()
        {
            // Arrange
            var doctor = CreateDoctor(isActive: false);
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                new DateTimeOffset(
                    2026, 9, 7, 10, 0, 0, TimeSpan.Zero));

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_AppointmentTypeInactive_ReturnsValidation()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(isActive: false);

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                new DateTimeOffset(
                    2026, 9, 7, 10, 0, 0, TimeSpan.Zero));

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_CrossesMidnight_ReturnsValidation()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(
                durationMinutes: 90);

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var startTime = new DateTimeOffset(
                2026, 9, 7, 23, 30, 0, TimeSpan.Zero);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }


        [Fact]
        public async Task CreateAppointmentAsync_OutsideWorkingHours_ReturnsValidation()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(9, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);

            unitOfWork.Verify(
                u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>(),
                    It.IsAny<IsolationLevel>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentAsync_ConflictingAppointment_ReturnsConflict()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(18, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateAppointment(
                    doctor.Id,
                    Guid.NewGuid(),
                    clinicLocation.Id,
                    appointmentType.Id,
                    startTime.AddMinutes(15),
                    startTime.AddMinutes(45))
                ]);

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);

            unitOfWork.Verify(
                u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>(),
                    It.IsAny<IsolationLevel>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.RollbackTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.CommitTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);

            appointmentRepository.Verify(
                r => r.AddAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(18, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            appointmentRepository
                .Setup(r => r.AddAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            appointmentRepository.Verify(
                r => r.AddAsync(
                    It.Is<Appointment>(a =>
                        a.DoctorId == doctor.Id &&
                        a.PatientId == patient.Id &&
                        a.ClinicLocationId == clinicLocation.Id &&
                        a.AppointmentTypeId == appointmentType.Id &&
                        a.StartTime == startTime &&
                        a.EndTime == startTime.AddMinutes(
                            appointmentType.DurationMinutes) &&
                        a.Status == AppointmentStatus.Confirmed),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.BeginTransactionAsync(
                    It.IsAny<CancellationToken>(),
                    It.IsAny<IsolationLevel>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.CommitTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.RollbackTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentAsync_AddAppointmentThrowsDbUpdateException_ReturnsConflict()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(18, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            appointmentRepository
                .Setup(r => r.AddAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new DbUpdateException());

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);

            unitOfWork.Verify(
                u => u.RollbackTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.CommitTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentAsync_AddAppointmentThrowsUnexpectedException_RethrowsException()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var doctorRepository = new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var patientRepository = new Mock<IPatientRepository>();

            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    patient.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(patient);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(8, 0),
                    new TimeOnly(18, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            appointmentRepository
                .Setup(r => r.AddAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(
                    new InvalidOperationException("Unexpected error"));

            var unitOfWork =
                new Mock<IUnitOfWork>();

            SetupUnitOfWork(unitOfWork);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new CreateAppointmentRequest(
                patient.Id,
                doctor.Id,
                clinicLocation.Id,
                appointmentType.Id,
                startTime);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.CreateAppointmentAsync(
                    request,
                    CancellationToken.None));

            unitOfWork.Verify(
                u => u.RollbackTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Once);

            unitOfWork.Verify(
                u => u.CommitTransactionAsync(
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetAppointmentByIdAsync_AppointmentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var appointmentId = Guid.NewGuid();

            appointmentRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            var doctorRepository = new Mock<IDoctorRepository>();
            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            // Act
            var result = await sut.GetAppointmentByIdAsync(
                appointmentId,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetAppointmentByIdAsync_AppointmentExists_ReturnsAppointmentResponse()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            var appointment = new Appointment
            {
                Id = appointmentId,
                DoctorId = Guid.NewGuid(),
                PatientId = Guid.NewGuid(),
                ClinicLocationId = Guid.NewGuid(),
                AppointmentTypeId = Guid.NewGuid(),
                StartTime = new DateTimeOffset(
                    2026, 9, 7, 10, 0, 0, TimeSpan.Zero),
                EndTime = new DateTimeOffset(
                    2026, 9, 7, 10, 30, 0, TimeSpan.Zero),
                Status = AppointmentStatus.Confirmed,
                CreatedAt = new DateTimeOffset(
                    2026, 9, 5, 10, 0, 0, TimeSpan.Zero)
            };

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var doctorRepository = new Mock<IDoctorRepository>();
            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            // Act
            var result = await sut.GetAppointmentByIdAsync(
                appointmentId,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(
                appointment.Id,
                result.Value.Id);

            Assert.Equal(
                appointment.DoctorId,
                result.Value.DoctorId);

            Assert.Equal(
                appointment.PatientId,
                result.Value.PatientId);

            Assert.Equal(
                appointment.ClinicLocationId,
                result.Value.ClinicLocationId);

            Assert.Equal(
                appointment.AppointmentTypeId,
                result.Value.AppointmentTypeId);

            Assert.Equal(
                appointment.StartTime,
                result.Value.StartTime);

            Assert.Equal(
                appointment.EndTime,
                result.Value.EndTime);

            Assert.Equal(
                appointment.Status,
                result.Value.Status);

            Assert.Equal(
                appointment.CreatedAt,
                result.Value.CreatedAt);
        }



        [Fact]
        public async Task CancelAppointmentAsync_AppointmentDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var appointmentId = Guid.NewGuid();

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Appointment?)null);

            var doctorRepository = new Mock<IDoctorRepository>();
            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            // Act
            var result = await sut.CancelAppointmentAsync(
                appointmentId,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);

            appointmentRepository.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CancelAppointmentAsync_AlreadyCancelled_ReturnsConflict()
        {
            // Arrange
            var appointment = CreateAppointment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30),
                AppointmentStatus.Cancelled);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByIdAsync(
                    appointment.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var doctorRepository = new Mock<IDoctorRepository>();
            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            // Act
            var result = await sut.CancelAppointmentAsync(
                appointment.Id,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);

            appointmentRepository.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CancelAppointmentAsync_CompletedAppointment_ReturnsConflict()
        {
            // Arrange
            var appointment = CreateAppointment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30),
                AppointmentStatus.Completed);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByIdAsync(
                    appointment.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            var doctorRepository = new Mock<IDoctorRepository>();
            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            // Act
            var result = await sut.CancelAppointmentAsync(
                appointment.Id,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);

            appointmentRepository.Verify(
                r => r.UpdateAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CancelAppointmentAsync_ValidAppointment_CancelsAppointment()
        {
            // Arrange
            var appointment = CreateAppointment(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30),
                AppointmentStatus.Confirmed);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByIdAsync(
                    appointment.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointment);

            appointmentRepository
                .Setup(r => r.UpdateAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var doctorRepository = new Mock<IDoctorRepository>();
            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            // Act
            var result = await sut.CancelAppointmentAsync(
                appointment.Id,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(
                AppointmentStatus.Cancelled,
                appointment.Status);

            Assert.NotNull(appointment.CancelledAt);

            appointmentRepository.Verify(
                r => r.UpdateAsync(
                    appointment,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }



        [Fact]
        public async Task GetAvailableSlotsAsync_DoctorDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctorId = Guid.NewGuid();

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var patientRepository = new Mock<IPatientRepository>();
            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : Guid.NewGuid(),
                AppointmentTypeId : Guid.NewGuid(),
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctorId,
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_ClinicLocationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocationId = Guid.NewGuid();

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicLocation?)null);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();
            var patientRepository =
                new Mock<IPatientRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocationId,
                AppointmentTypeId : Guid.NewGuid(),
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_AppointmentTypeDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentTypeId = Guid.NewGuid();

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentTypeId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentType?)null);

            var patientRepository =
                new Mock<IPatientRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentTypeId,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_DoctorNotAssignedToClinic_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var patientRepository =
                new Mock<IPatientRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_DoctorInactive_ReturnsValidation()
        {
            // Arrange
            var doctor = CreateDoctor(isActive: false);
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var patientRepository =
                new Mock<IPatientRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_AppointmentTypeInactive_ReturnsNotFound()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType =
                CreateAppointmentType(isActive: false);

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var patientRepository =
                new Mock<IPatientRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository =
                new Mock<IAppointmentRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);

            // This is NotFound because the service currently returns
            // ErrorType.NotFound for inactive appointment types.
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_NoWorkingHours_ReturnsEmptyList()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var patientRepository =
                new Mock<IPatientRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_ValidWorkingHours_GeneratesSlots()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(
                durationMinutes: 30);

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(11, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var patientRepository =
                new Mock<IPatientRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            // 09:00, 09:30, 10:00, 10:30
            Assert.Equal(4, result.Value.Count);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_MultipleWorkingHours_GeneratesSlotsFromAllBlocks()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(
                durationMinutes: 30);

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(10, 0)),

                CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(14, 0),
                    new TimeOnly(15, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var patientRepository =
                new Mock<IPatientRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            // 09:00, 09:30, 14:00, 14:30
            Assert.Equal(4, result.Value.Count);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_ExistingAppointmentOverlapsSlot_RemovesSlot()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(
                durationMinutes: 60);

            var date = new DateOnly(2026, 9, 7);

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(12, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            var existingAppointment = CreateAppointment(
                doctor.Id,
                Guid.NewGuid(),
                clinicLocation.Id,
                appointmentType.Id,
                new DateTimeOffset(
                    2026, 9, 7, 10, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(
                    2026, 9, 7, 11, 0, 0, TimeSpan.Zero));

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    date,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([existingAppointment]);

            var patientRepository =
                new Mock<IPatientRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : date
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            // Original slots:
            // 09:00-10:00
            // 10:00-11:00  <-- conflict
            // 11:00-12:00
            //
            // Expected remaining:
            // 09:00-10:00
            // 11:00-12:00

            Assert.Equal(2, result.Value.Count);

            Assert.DoesNotContain(
                result.Value,
                slot =>
                    slot.StartTime ==
                    new DateTimeOffset(
                        2026, 9, 7, 10, 0, 0, TimeSpan.Zero));
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_PastSlots_AreFilteredOut()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(
                durationMinutes: 30);

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    It.IsAny<DayOfWeek>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Wednesday,
                    new TimeOnly(9, 0),
                    new TimeOnly(11, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var patientRepository =
                new Mock<IPatientRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2020, 1, 1)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }


        [Fact]
        public async Task GetAvailableSlotsAsync_SlotExactlyAtWorkingHourBoundary_IsGenerated()
        {
            // Arrange
            var doctor = CreateDoctor();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType(
                durationMinutes: 60);

            var doctorRepository =
                new Mock<IDoctorRepository>();

            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var clinicLocationRepository =
                new Mock<IClinicLocationRepository>();

            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository =
                new Mock<IAppointmentTypeRepository>();

            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    doctor.Id,
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            doctorWorkingHourRepository
                .Setup(r => r.GetByDoctorAndDayAsync(
                    doctor.Id,
                    DayOfWeek.Monday,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                [
                    CreateWorkingHour(
                    doctor.Id,
                    DayOfWeek.Monday,
                    new TimeOnly(9, 0),
                    new TimeOnly(10, 0))
                ]);

            var appointmentRepository =
                new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            var patientRepository =
                new Mock<IPatientRepository>();
            var unitOfWork =
                new Mock<IUnitOfWork>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository,
                unitOfWork);

            var request = new GetAvailableSlotsRequest
            (
                ClinicLocationId : clinicLocation.Id,
                AppointmentTypeId : appointmentType.Id,
                Date : new DateOnly(2026, 9, 7)
            );

            // Act
            var result = await sut.GetAvailableSlotsAsync(
                doctor.Id,
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);

            Assert.Equal(
                new DateTimeOffset(
                    2026, 9, 7, 9, 0, 0, TimeSpan.Zero),
                result.Value[0].StartTime);

            Assert.Equal(
                new DateTimeOffset(
                    2026, 9, 7, 10, 0, 0, TimeSpan.Zero),
                result.Value[0].EndTime);
        }
    }
}
