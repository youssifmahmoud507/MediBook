using MediBook.Application.Common;
using MediBook.Application.DTOs.DoctorSpecializations;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class DoctorSpecializationServiceTests
    {
        private readonly Mock<IDoctorSpecializationRepository> _doctorSpecializationRepository;
        private readonly Mock<ISpecialtyRepository> _specialtyRepository;
        private readonly Mock<IDoctorRepository> _doctorRepository;

        private readonly DoctorSpecializationService _sut;

        public DoctorSpecializationServiceTests()
        {
            _doctorSpecializationRepository = new Mock<IDoctorSpecializationRepository>();
            _specialtyRepository = new Mock<ISpecialtyRepository>();
            _doctorRepository = new Mock<IDoctorRepository>();

            _sut = new DoctorSpecializationService(
                _doctorSpecializationRepository.Object,
                _specialtyRepository.Object,
                _doctorRepository.Object);
        }

        // =========================================================
        // AssignSpecialtyToDoctorAsync
        // =========================================================

        [Fact]
        public async Task AssignSpecialtyToDoctorAsync_WhenDoctorDoesNotExist_ReturnsNotFound()
        {
            var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();

            var request = new AssignSpecialtyToDoctorRequest
            (
                SpecialtyId : specialtyId,
                IsPrimary : true
            );

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Doctor?)null);

            var result = await _sut.AssignSpecialtyToDoctorAsync(
                doctorId,
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Doctor not found", result.Error);

            _specialtyRepository.Verify(
                x => x.GetByIdAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _doctorSpecializationRepository.Verify(
                x => x.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _doctorSpecializationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorSpecialization>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignSpecialtyToDoctorAsync_WhenSpecialtyDoesNotExist_ReturnsNotFound()
        {
            var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);

            var request = new AssignSpecialtyToDoctorRequest
            (
                SpecialtyId : specialtyId,
                IsPrimary : true
            );

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _specialtyRepository
                .Setup(x => x.GetByIdAsync(
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Specialty?)null);

            var result = await _sut.AssignSpecialtyToDoctorAsync(
                doctorId,
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Specialty not found", result.Error);

            _doctorSpecializationRepository.Verify(
                x => x.ExistsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);

            _doctorSpecializationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorSpecialization>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignSpecialtyToDoctorAsync_WhenSpecialtyAlreadyAssigned_ReturnsConflict()
        {
            var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);
            var specialty = CreateSpecialty(specialtyId);

            var request = new AssignSpecialtyToDoctorRequest
            (
                SpecialtyId : specialtyId,
                IsPrimary : true
            );

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _specialtyRepository
                .Setup(x => x.GetByIdAsync(
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(specialty);

            _doctorSpecializationRepository
                .Setup(x => x.ExistsAsync(
                    doctorId,
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.AssignSpecialtyToDoctorAsync(
                doctorId,
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal(
                "This specialty is already assigned to this doctor.",
                result.Error);

            _doctorSpecializationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorSpecialization>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AssignSpecialtyToDoctorAsync_WhenValid_CreatesAssignmentAndReturnsSuccess()
        {
            var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);
            var specialty = CreateSpecialty(specialtyId);

            var request = new AssignSpecialtyToDoctorRequest
            (
                SpecialtyId: specialtyId,
                IsPrimary: true
            );

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _specialtyRepository
                .Setup(x => x.GetByIdAsync(
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(specialty);

            _doctorSpecializationRepository
                .Setup(x => x.ExistsAsync(
                    doctorId,
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            DoctorSpecialization? addedSpecialization = null;

            _doctorSpecializationRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<DoctorSpecialization>(),
                    It.IsAny<CancellationToken>()))
                .Callback<DoctorSpecialization, CancellationToken>(
                    (assignment, _) =>
                    {
                        addedSpecialization = assignment;
                    })
                .Returns(Task.CompletedTask);

            var result = await _sut.AssignSpecialtyToDoctorAsync(
                doctorId,
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);

            Assert.NotNull(addedSpecialization);

            Assert.Equal(
                doctorId,
                addedSpecialization!.DoctorId);

            Assert.Equal(
                specialtyId,
                addedSpecialization.SpecializationId);

            Assert.Equal(
                request.IsPrimary,
                addedSpecialization.IsPrimary);

            _doctorSpecializationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<DoctorSpecialization>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AssignSpecialtyToDoctorAsync_WhenIsPrimaryIsFalse_CreatesNonPrimaryAssignment()
        {
            var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();

            var doctor = CreateDoctor(doctorId);
            var specialty = CreateSpecialty(specialtyId);

            var request = new AssignSpecialtyToDoctorRequest(
                specialtyId,
                false);

            Assert.False(request.IsPrimary);

            _doctorRepository
                .Setup(x => x.GetByIdAsync(
                    doctorId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(doctor);

            _specialtyRepository
                .Setup(x => x.GetByIdAsync(
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(specialty);

            _doctorSpecializationRepository
                .Setup(x => x.ExistsAsync(
                    doctorId,
                    specialtyId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            DoctorSpecialization? addedSpecialization = null;

            _doctorSpecializationRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<DoctorSpecialization>(),
                    It.IsAny<CancellationToken>()))
                .Callback<DoctorSpecialization, CancellationToken>(
                    (assignment, _) =>
                    {
                        addedSpecialization = assignment;
                    })
                .Returns(Task.CompletedTask);

            var result = await _sut.AssignSpecialtyToDoctorAsync(
                doctorId,
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(addedSpecialization);

            Assert.Equal(doctorId, addedSpecialization!.DoctorId);
            Assert.Equal(specialtyId, addedSpecialization.SpecializationId);
            Assert.False(addedSpecialization.IsPrimary);
        }
        // =========================================================
        // Helpers
        // =========================================================

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

        private static Specialty CreateSpecialty(Guid id)
        {
            return new Specialty
            {
                Id = id,
                Name = "Cardiology"
            };
        }
    }
}
