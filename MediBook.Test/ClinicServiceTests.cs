using MediBook.Application.Common;
using MediBook.Application.DTOs.Clinics;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class ClinicServiceTests
    {
        private readonly Mock<IClinicRepository> _clinicRepository;

        private readonly ClinicService _sut;

        public ClinicServiceTests()
        {
            _clinicRepository = new Mock<IClinicRepository>();

            _sut = new ClinicService(
                _clinicRepository.Object);
        }

        [Fact]
        public async Task AddAsync_WhenNameIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(name: "");

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber and Email are required fields.",
                result.Error);

            VerifyClinicWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenPhoneNumberIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(phoneNumber: "");

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber and Email are required fields.",
                result.Error);

            VerifyClinicWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenEmailIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(email: "");

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber and Email are required fields.",
                result.Error);

            VerifyClinicWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenRequiredFieldIsWhitespace_ReturnsValidationFailure()
        {
            var request = CreateRequest(name: "   ");

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber and Email are required fields.",
                result.Error);

            VerifyClinicWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenEmailAlreadyExists_ReturnsConflict()
        {
            var request = CreateRequest();

            _clinicRepository
                .Setup(x => x.ExistsWithEmailAsync(
                    request.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal(
                "A clinic with the same email already exists.",
                result.Error);

            VerifyClinicWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenRequestIsValid_CreatesClinicAndReturnsId()
        {
            var request = CreateRequest();

            _clinicRepository
                .Setup(x => x.ExistsWithEmailAsync(
                    request.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Clinic? addedClinic = null;

            _clinicRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Clinic>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Clinic, CancellationToken>((clinic, _) =>
                {
                    addedClinic = clinic;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(addedClinic);

            Assert.Equal(result.Value, addedClinic!.Id);
            Assert.Equal(request.Name, addedClinic.Name);
            Assert.Equal(request.Description, addedClinic.Description);
            Assert.Equal(request.PhoneNumber, addedClinic.PhoneNumber);
            Assert.Equal(request.Email, addedClinic.Email);

            Assert.True(addedClinic.IsActive);

            _clinicRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Clinic>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrows_PropagatesException()
        {
            var request = CreateRequest();

            _clinicRepository
                .Setup(x => x.ExistsWithEmailAsync(
                    request.Email,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _clinicRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Clinic>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _sut.AddAsync(
                    request,
                    CancellationToken.None));

            Assert.Equal("Database error", exception.Message);
        }


        [Fact]
        public async Task GetByIdResponseAsync_WhenClinicDoesNotExist_ReturnsNotFound()
        {
            var id = Guid.NewGuid();

            _clinicRepository
                .Setup(x => x.GetByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Clinic?)null);

            var result = await _sut.GetByIdResponseAsync(
                id,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal(
                "Clinic not found.",
                result.Error);
        }

        [Fact]
        public async Task GetByIdResponseAsync_WhenClinicExists_ReturnsMappedResponse()
        {
            var clinic = CreateClinic();

            _clinicRepository
                .Setup(x => x.GetByIdAsync(
                    clinic.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinic);

            var result = await _sut.GetByIdResponseAsync(
                clinic.Id,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(clinic.Id, result.Value.Id);
            Assert.Equal(clinic.Name, result.Value.Name);
            Assert.Equal(clinic.Description, result.Value.Description);
            Assert.Equal(clinic.PhoneNumber, result.Value.PhoneNumber);
            Assert.Equal(clinic.Email, result.Value.Email);
        }

        [Fact]
        public async Task GetByIdResponseAsync_WhenRepositoryThrows_PropagatesException()
        {
            var id = Guid.NewGuid();

            _clinicRepository
                .Setup(x => x.GetByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _sut.GetByIdResponseAsync(
                    id,
                    CancellationToken.None));

            Assert.Equal("Database error", exception.Message);
        }
        private void VerifyClinicWasNotAdded()
        {
            _clinicRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Clinic>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static ClinicRequest CreateRequest(string? name = null,string? phoneNumber = null,string? email = null,string? description = null)
        {
            return new ClinicRequest(
                Name: name ?? "MediBook Clinic",
                Description: description ?? "General medical clinic",
                PhoneNumber: phoneNumber ?? "0220000000",
                Email: email ?? "clinic@medibook.com");
        }

        private static Clinic CreateClinic()
        {
            return new Clinic
            {
                Id = Guid.NewGuid(),
                Name = "MediBook Clinic",
                Description = "General medical clinic",
                PhoneNumber = "0220000000",
                Email = "clinic@medibook.com",
                CreatedAt = DateTimeOffset.UtcNow,
                IsActive = true
            };
        }
    }
}
