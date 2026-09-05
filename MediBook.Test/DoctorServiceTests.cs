using MediBook.Application.Common;
using MediBook.Application.DTOs.Doctors;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class DoctorServiceTests
    {
        private readonly Mock<IDoctorRepository> _doctorRepository;
        private readonly Mock<IDoctorWorkingHourRepository> _doctorWorkingHourRepository;
        private readonly DoctorService _sut;

        public DoctorServiceTests()
        {
            _doctorRepository = new Mock<IDoctorRepository>();
            _doctorWorkingHourRepository = new Mock<IDoctorWorkingHourRepository>();

            _sut = new DoctorService(
                _doctorRepository.Object,
                _doctorWorkingHourRepository.Object);
        }

        [Fact]
        public async Task AddDoctorAsync_WhenRequestIsNull_ReturnsValidationError()
        {
            var result = await _sut.AddDoctorAsync(
                null!,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid doctor request", result.Error);

            _doctorRepository.Verify(
                x => x.ExistsWithLicenseNumberAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _doctorRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Theory]
        [InlineData(null, "Doe", "LIC-001", "01000000000", "doctor@test.com")]
        [InlineData("John", null, "LIC-001", "01000000000", "doctor@test.com")]
        [InlineData("John", "Doe", null, "01000000000", "doctor@test.com")]
        [InlineData("John", "Doe", "LIC-001", null, "doctor@test.com")]
        [InlineData("John", "Doe", "LIC-001", "01000000000", null)]
        public async Task AddDoctorAsync_WhenRequiredFieldIsNull_ReturnsValidationError(string? firstName,string? lastName,string? licenseNumber,string? phoneNumber,string? email)
        {
            var request = new DoctorRequest
            (
                FirstName : firstName!,
                LastName : lastName!,
                LicenseNumber : licenseNumber!,
                PhoneNumber : phoneNumber!,
                Email : email!
            );

            var result = await _sut.AddDoctorAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid doctor request", result.Error);

            _doctorRepository.Verify(
                x => x.ExistsWithLicenseNumberAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _doctorRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddDoctorAsync_WhenLicenseAlreadyExists_ReturnsConflict()
        {
            var request = CreateDoctorRequest();

            _doctorRepository
                .Setup(x => x.ExistsWithLicenseNumberAsync(
                    request.LicenseNumber,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.AddDoctorAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal(
                "A doctor with the same license number already exists.",
                result.Error);

            _doctorRepository.Verify(
                x => x.ExistsWithLicenseNumberAsync(
                    request.LicenseNumber,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _doctorRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddDoctorAsync_WhenValid_CreatesDoctorAndReturnsSuccess()
        {
            var request = CreateDoctorRequest();

            _doctorRepository
                .Setup(x => x.ExistsWithLicenseNumberAsync(
                    request.LicenseNumber,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Doctor? addedDoctor = null;

            _doctorRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Doctor, CancellationToken>((doctor, _) =>
                {
                    addedDoctor = doctor;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.AddDoctorAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(addedDoctor);

            Assert.Equal(result.Value, addedDoctor!.Id);
            Assert.Equal(request.FirstName, addedDoctor.FirstName);
            Assert.Equal(request.LastName, addedDoctor.LastName);
            Assert.Equal(request.LicenseNumber, addedDoctor.LicenseNumber);
            Assert.Equal(request.PhoneNumber, addedDoctor.PhoneNumber);
            Assert.Equal(request.Email, addedDoctor.Email);

            Assert.True(addedDoctor.IsActive);
            Assert.NotEqual(default, addedDoctor.CreatedAt);

            _doctorRepository.Verify(
                x => x.ExistsWithLicenseNumberAsync(
                    request.LicenseNumber,
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _doctorRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorByIdAsync_WhenDoctorDoesNotExist_ReturnsNotFound()
        {
            var doctorId = Guid.NewGuid();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var result = await _sut.GetDoctorByIdAsync(
                doctorId,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Doctor not found", result.Error);

            _doctorRepository.Verify(
                x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorByIdAsync_WhenDoctorExists_ReturnsMappedResponse()
        {
            var doctor = CreateDoctor();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            var result = await _sut.GetDoctorByIdAsync(
                doctor.Id,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(doctor.Id, result.Value!.Id);
            Assert.Equal(doctor.FirstName, result.Value.FirstName);
            Assert.Equal(doctor.LastName, result.Value.LastName);
            Assert.Equal(doctor.LicenseNumber, result.Value.LicenseNumber);
            Assert.Equal(doctor.PhoneNumber, result.Value.PhoneNumber);
            Assert.Equal(doctor.Email, result.Value.Email);

            _doctorRepository.Verify(
                x => x.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorWithWorkingHoursAsync_WhenDoctorDoesNotExist_ReturnsNotFound()
        {
            var doctorId = Guid.NewGuid();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var result = await _sut.GetDoctorWithWorkingHoursAsync(
                doctorId,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Doctor not found", result.Error);

            _doctorWorkingHourRepository.Verify(
                x => x.GetByDoctorAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetDoctorWithWorkingHoursAsync_WhenDoctorExistsWithWorkingHours_ReturnsMappedResponse()
        {
            var doctor = CreateDoctor();

            var workingHours = new List<DoctorWorkingHour>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Saturday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Sunday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(15, 0),
                IsActive = true
            }
        };

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _doctorWorkingHourRepository
                .Setup(x => x.GetByDoctorAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(workingHours);

            var result = await _sut.GetDoctorWithWorkingHoursAsync(
                doctor.Id,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(doctor.Id, result.Value!.Id);
            Assert.Equal(doctor.FirstName, result.Value.FirstName);
            Assert.Equal(doctor.LastName, result.Value.LastName);
            Assert.Equal(doctor.LicenseNumber, result.Value.LicenseNumber);
            Assert.Equal(doctor.PhoneNumber, result.Value.PhoneNumber);
            Assert.Equal(doctor.Email, result.Value.Email);

            Assert.NotNull(result.Value.WorkingHours);
            Assert.Equal(2, result.Value.WorkingHours.Count);

            Assert.Equal(
                DayOfWeek.Saturday,
                result.Value.WorkingHours[0].DayOfWeek);

            Assert.Equal(
                new TimeOnly(9, 0),
                result.Value.WorkingHours[0].StartTime);

            Assert.Equal(
                new TimeOnly(17, 0),
                result.Value.WorkingHours[0].EndTime);

            Assert.Equal(
                DayOfWeek.Sunday,
                result.Value.WorkingHours[1].DayOfWeek);

            Assert.Equal(
                new TimeOnly(10, 0),
                result.Value.WorkingHours[1].StartTime);

            Assert.Equal(
                new TimeOnly(15, 0),
                result.Value.WorkingHours[1].EndTime);

            _doctorWorkingHourRepository.Verify(
                x => x.GetByDoctorAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetDoctorWithWorkingHoursAsync_WhenDoctorHasNoWorkingHours_ReturnsEmptyList()
        {
            var doctor = CreateDoctor();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _doctorWorkingHourRepository
                .Setup(x => x.GetByDoctorAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DoctorWorkingHour>());

            var result = await _sut.GetDoctorWithWorkingHoursAsync(
                doctor.Id,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.NotNull(result.Value!.WorkingHours);
            Assert.Empty(result.Value.WorkingHours);

            _doctorWorkingHourRepository.Verify(
                x => x.GetByDoctorAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        private static DoctorRequest CreateDoctorRequest()
        {
            return new DoctorRequest
            (
                FirstName : "John",
                LastName : "Doe",
                LicenseNumber : "LIC-001",
                PhoneNumber : "01000000000",
                Email : "doctor@test.com"
            );
        }

        private static Doctor CreateDoctor()
        {
            return new Doctor
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                LicenseNumber = "LIC-001",
                PhoneNumber = "01000000000",
                Email = "doctor@test.com",
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
        }
    }
}
