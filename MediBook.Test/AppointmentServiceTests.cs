using MediBook.Application.Common;
using MediBook.Application.DTOs.Appointments;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using MediBook.Domain.Enums;
using MediBook.Infrustructure.Services;
using Moq;

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
        private static ClinicLocation CreateClinicLocation(string timeZoneId = "Egypt Standard Time")
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
        private static DoctorWorkingHour CreateWorkingHour(Guid doctorId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime)
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
        private static AppointmentService CreateSut(Mock<IAppointmentRepository> appointmentRepository, Mock<IDoctorRepository> doctorRepository, Mock<IPatientRepository> patientRepository, Mock<IClinicLocationRepository> clinicLocationRepository, Mock<IAppointmentTypeRepository> appointmentTypeRepository, Mock<IDoctorClinicAssignmentRepository> doctorClinicAssignmentRepository, Mock<IDoctorWorkingHourRepository> doctorWorkingHourRepository)
        {
            return new AppointmentService(
                appointmentRepository.Object,
                doctorRepository.Object,
                patientRepository.Object,
                clinicLocationRepository.Object,
                appointmentTypeRepository.Object,
                doctorClinicAssignmentRepository.Object,
                doctorWorkingHourRepository.Object);
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
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

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
            var doctorRepository = new Mock<IDoctorRepository>();
            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor());

            var patientRepository = new Mock<IPatientRepository>();
            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Patient?)null);

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

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
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicLocation?)null);

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

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
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateClinicLocation());

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentType?)null);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

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
            var doctorRepository = new Mock<IDoctorRepository>();
            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor());

            var patientRepository = new Mock<IPatientRepository>();
            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateClinicLocation());

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateAppointmentType());

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();

            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

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
        public async Task CreateAppointmentAsync_StartTimeInPast_ReturnsValidation()
        {
            // Arrange
            var doctorRepository = new Mock<IDoctorRepository>();
            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor());

            var patientRepository = new Mock<IPatientRepository>();
            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateClinicLocation());

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateAppointmentType());

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

            var request = CreateRequest(
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
            var doctorRepository = new Mock<IDoctorRepository>();
            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor(isActive: false));

            var patientRepository = new Mock<IPatientRepository>();
            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateClinicLocation());

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateAppointmentType());

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

            var request = CreateRequest();

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
            var doctorRepository = new Mock<IDoctorRepository>();
            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor());

            var patientRepository = new Mock<IPatientRepository>();
            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateClinicLocation());

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateAppointmentType(isActive: false));

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();
            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

            var request = CreateRequest();

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
            var doctorRepository = new Mock<IDoctorRepository>();
            doctorRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateDoctor());

            var patientRepository = new Mock<IPatientRepository>();
            patientRepository
                .Setup(r => r.GetPatientByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePatient());

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateClinicLocation());

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreateAppointmentType(
                    durationMinutes: 90));

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var doctorWorkingHourRepository =
                new Mock<IDoctorWorkingHourRepository>();

            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

            var request = CreateRequest(
                DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(23));

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

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
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
                .ReturnsAsync(new List<DoctorWorkingHour>
                {
            new DoctorWorkingHour
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(08, 0),
                EndTime = new TimeOnly(09, 0),
                IsActive = true
            }
                });

            var appointmentRepository = new Mock<IAppointmentRepository>();

            var sut = new AppointmentService(
                appointmentRepository.Object,
                doctorRepository.Object,
                patientRepository.Object,
                clinicLocationRepository.Object,
                appointmentTypeRepository.Object,
                doctorClinicAssignmentRepository.Object,
                doctorWorkingHourRepository.Object);

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var request = new CreateAppointmentRequest(
                PatientId: patient.Id,
                DoctorId: doctor.Id,
                ClinicLocationId: clinicLocation.Id,
                AppointmentTypeId: appointmentType.Id,
                StartTime: startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ConflictingAppointment_ReturnsConflict()
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

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
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
                .ReturnsAsync(new List<DoctorWorkingHour>
                {
            new DoctorWorkingHour
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(18, 0),
                IsActive = true
            }
                });

            var appointmentRepository = new Mock<IAppointmentRepository>();

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>
                {
            new Appointment
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                PatientId = Guid.NewGuid(),
                ClinicLocationId = clinicLocation.Id,
                AppointmentTypeId = appointmentType.Id,
                StartTime = startTime.AddMinutes(15),
                EndTime = startTime.AddMinutes(45),
                Status = AppointmentStatus.Confirmed
            }
                });

            var sut = new AppointmentService(
                appointmentRepository.Object,
                doctorRepository.Object,
                patientRepository.Object,
                clinicLocationRepository.Object,
                appointmentTypeRepository.Object,
                doctorClinicAssignmentRepository.Object,
                doctorWorkingHourRepository.Object);

            var request = new CreateAppointmentRequest(
                PatientId: patient.Id,
                DoctorId: doctor.Id,
                ClinicLocationId: clinicLocation.Id,
                AppointmentTypeId: appointmentType.Id,
                StartTime: startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
        }

        [Fact]
        public async Task CreateAppointmentAsync_ValidRequest_ReturnsSuccess()
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

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
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
                .ReturnsAsync(new List<DoctorWorkingHour>
                {
            new DoctorWorkingHour
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(8, 0),
                EndTime = new TimeOnly(18, 0),
                IsActive = true
            }
                });

            var appointmentRepository = new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Appointment>());

            var sut = new AppointmentService(
                appointmentRepository.Object,
                doctorRepository.Object,
                patientRepository.Object,
                clinicLocationRepository.Object,
                appointmentTypeRepository.Object,
                doctorClinicAssignmentRepository.Object,
                doctorWorkingHourRepository.Object);

            var startTime = new DateTimeOffset(
                2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

            var request = new CreateAppointmentRequest(
                PatientId: patient.Id,
                DoctorId: doctor.Id,
                ClinicLocationId: clinicLocation.Id,
                AppointmentTypeId: appointmentType.Id,
                StartTime: startTime);

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
                        a.StartTime == startTime),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task CreateAppointmentAsync_ValidRequest_AddsAppointment()
        {
            // Arrange
            var doctor = CreateDoctor();
            var patient = CreatePatient();
            var clinicLocation = CreateClinicLocation();
            var appointmentType = CreateAppointmentType();
            var startTime = DateTimeOffset.UtcNow.Date.AddDays(1).AddHours(10);

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

            var clinicLocationRepository = new Mock<IClinicLocationRepository>();
            clinicLocationRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var appointmentTypeRepository = new Mock<IAppointmentTypeRepository>();
            appointmentTypeRepository
                .Setup(r => r.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            var doctorClinicAssignmentRepository =
                new Mock<IDoctorClinicAssignmentRepository>();
            doctorClinicAssignmentRepository
                .Setup(r => r.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
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
                    startTime.DayOfWeek,
                    new TimeOnly(9, 0),
                    new TimeOnly(17, 0))
                ]);

            var appointmentRepository = new Mock<IAppointmentRepository>();

            appointmentRepository
                .Setup(r => r.GetByDoctorAndDateAsync(
                    doctor.Id,
                    It.IsAny<DateOnly>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            Appointment? addedAppointment = null;

            appointmentRepository
                .Setup(r => r.AddAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Appointment, CancellationToken>(
                    (appointment, _) => addedAppointment = appointment)
                .Returns(Task.CompletedTask);

            var sut = CreateSut(
                appointmentRepository,
                doctorRepository,
                patientRepository,
                clinicLocationRepository,
                appointmentTypeRepository,
                doctorClinicAssignmentRepository,
                doctorWorkingHourRepository);

            var request = new CreateAppointmentRequest(
                PatientId: patient.Id,
                DoctorId: doctor.Id,
                ClinicLocationId: clinicLocation.Id,
                AppointmentTypeId: appointmentType.Id,
                StartTime: startTime);

            // Act
            var result = await sut.CreateAppointmentAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(addedAppointment);

            Assert.Equal(request.DoctorId, addedAppointment!.DoctorId);
            Assert.Equal(request.PatientId, addedAppointment.PatientId);
            Assert.Equal(request.ClinicLocationId, addedAppointment.ClinicLocationId);
            Assert.Equal(request.AppointmentTypeId, addedAppointment.AppointmentTypeId);
            Assert.Equal(request.StartTime, addedAppointment.StartTime);
            Assert.Equal(
                request.StartTime.AddMinutes(appointmentType.DurationMinutes),
                addedAppointment.EndTime);
            Assert.Equal(
                AppointmentStatus.Confirmed,
                addedAppointment.Status);

            appointmentRepository.Verify(
                r => r.AddAsync(
                    It.IsAny<Appointment>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
