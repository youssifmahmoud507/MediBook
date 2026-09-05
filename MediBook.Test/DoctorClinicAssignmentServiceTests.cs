using MediBook.Application.Common;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class DoctorClinicAssignmentServiceTests
    {
        private readonly Mock<IDoctorClinicAssignmentRepository> _assignmentRepository;
        private readonly Mock<IDoctorRepository> _doctorRepository;
        private readonly Mock<IClinicLocationRepository> _clinicLocationRepository;

        private readonly DoctorClinicAssignmentService _sut;

        public DoctorClinicAssignmentServiceTests()
        {
            _assignmentRepository = new Mock<IDoctorClinicAssignmentRepository>();
            _doctorRepository = new Mock<IDoctorRepository>();
            _clinicLocationRepository = new Mock<IClinicLocationRepository>();

            _sut = new DoctorClinicAssignmentService(
                _assignmentRepository.Object,
                _doctorRepository.Object,
                _clinicLocationRepository.Object);
        }

        [Fact]
        public async Task AssignDoctorToLocationAsync_WhenDoctorDoesNotExist_ReturnsNotFound()
        {
            var doctorId = Guid.NewGuid();
            var clinicLocationId = Guid.NewGuid();

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var result = await _sut.AssignDoctorToLocationAsync(
                doctorId,
                clinicLocationId,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Doctor not found", result.Error);

            _clinicLocationRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _assignmentRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorClinicAssignment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignDoctorToLocationAsync_WhenClinicLocationDoesNotExist_ReturnsNotFound()
        {
            var doctorId = Guid.NewGuid();
            var clinicLocationId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _clinicLocationRepository
                .Setup(x => x.GetByIdAsync(
                    clinicLocationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicLocation?)null);

            var result = await _sut.AssignDoctorToLocationAsync(
                doctorId,
                clinicLocationId,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Clinic location not found", result.Error);

            _assignmentRepository.Verify(
                x => x.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _assignmentRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorClinicAssignment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignDoctorToLocationAsync_WhenAlreadyAssigned_ReturnsConflict()
        {
            var doctorId = Guid.NewGuid();
            var clinicLocationId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);
            var clinicLocation = CreateClinicLocation(clinicLocationId);

            SetupDoctorExists(doctor);
            SetupClinicLocationExists(clinicLocation);

            _assignmentRepository
                .Setup(x => x.ExistsAsync(
                    doctorId,
                    clinicLocationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.AssignDoctorToLocationAsync(
                doctorId,
                clinicLocationId,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal(
                "This doctor is already assigned to this clinic location.",
                result.Error);

            _assignmentRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorClinicAssignment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignDoctorToLocationAsync_WhenValid_CreatesAssignmentAndReturnsSuccess()
        {
            var doctorId = Guid.NewGuid();
            var clinicLocationId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);
            var clinicLocation = CreateClinicLocation(clinicLocationId);

            SetupDoctorExists(doctor);
            SetupClinicLocationExists(clinicLocation);

            _assignmentRepository
                .Setup(x => x.ExistsAsync(
                    doctorId,
                    clinicLocationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            DoctorClinicAssignment? addedAssignment = null;

            _assignmentRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<DoctorClinicAssignment>(),
                    It.IsAny<CancellationToken>()))
                .Callback<DoctorClinicAssignment, CancellationToken>(
                    (assignment, _) =>
                    {
                        addedAssignment = assignment;
                    })
                .Returns(Task.CompletedTask);

            var result = await _sut.AssignDoctorToLocationAsync(
                doctorId,
                clinicLocationId,
                CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.NotNull(addedAssignment);
            Assert.Equal(doctorId, addedAssignment!.DoctorId);
            Assert.Equal(clinicLocationId, addedAssignment.ClinicLocationId);
            Assert.True(addedAssignment.IsActive);

            _assignmentRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorClinicAssignment>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignDoctorToLocationAsync_WhenRepositoryThrows_PropagatesException()
        {
            var doctorId = Guid.NewGuid();
            var clinicLocationId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);
            var clinicLocation = CreateClinicLocation(clinicLocationId);

            SetupDoctorExists(doctor);
            SetupClinicLocationExists(clinicLocation);

            _assignmentRepository
                .Setup(x => x.ExistsAsync(
                    doctorId,
                    clinicLocationId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _assignmentRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<DoctorClinicAssignment>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _sut.AssignDoctorToLocationAsync(
                    doctorId,
                    clinicLocationId,
                    CancellationToken.None));

            Assert.Equal("Database error", exception.Message);
        }
        private void SetupDoctorExists(Doctor doctor)
        {
            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctor.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);
        }

        private void SetupClinicLocationExists(ClinicLocation clinicLocation)
        {
            _clinicLocationRepository
                .Setup(x => x.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);
        }

        private static Doctor CreateDoctor(Guid id)
        {
            return new Doctor
            {
                Id = id,
                FirstName = "John",
                LastName = "Doe",
                PhoneNumber = "01000000000",
                Email = "doctor@test.com",
                LicenseNumber = "LIC-001",
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static ClinicLocation CreateClinicLocation(Guid id)
        {
            return new ClinicLocation
            {
                Id = id,
                ClinicId = Guid.NewGuid(),
                Name = "Main Clinic",
                AddressLine = "123 Main Street",
                City = "Cairo",
                Country = "Egypt",
                PhoneNumber = "0220000000",
                TimeZoneId = "Egypt Standard Time",
                IsActive = true
            };
        }
    }
}
