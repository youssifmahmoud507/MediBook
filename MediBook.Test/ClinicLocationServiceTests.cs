using MediBook.Application.Common;
using MediBook.Application.DTOs.ClinicLocations;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class ClinicLocationServiceTests
    {
        private readonly Mock<IClinicLocationRepository> _clinicLocationRepository;
        private readonly Mock<IClinicRepository> _clinicRepository;

        private readonly ClinicLocationService _sut;

        public ClinicLocationServiceTests()
        {
            _clinicLocationRepository = new Mock<IClinicLocationRepository>();
            _clinicRepository = new Mock<IClinicRepository>();

            _sut = new ClinicLocationService(
                _clinicLocationRepository.Object,
                _clinicRepository.Object);
        }

        [Fact]
        public async Task AddAsync_WhenClinicDoesNotExist_ReturnsNotFound()
        {
            var request = CreateRequest();

            _clinicRepository
                .Setup(x => x.GetByIdAsync(
                    request.ClinicId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((Clinic?)null);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal("Clinic not found", result.Error);

            _clinicLocationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<ClinicLocation>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task AddAsync_WhenNameIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(name: "");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenPhoneNumberIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(phoneNumber: "");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenTimeZoneIdIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(timeZoneId: "");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenAddressLineIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(addressLine: "");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenCityIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(city: "");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenCountryIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateRequest(country: "");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenRequiredFieldIsWhitespace_ReturnsValidationFailure()
        {
            var request = CreateRequest(name: "   ");

            SetupClinicExists(request.ClinicId);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal(
                "Name, PhoneNumber, TimeZoneId, AddressLine, City and Country are required fields.",
                result.Error);

            VerifyLocationWasNotAdded();
        }

        [Fact]
        public async Task AddAsync_WhenRequestIsValid_CreatesClinicLocationAndReturnsId()
        {
            var request = CreateRequest();

            SetupClinicExists(request.ClinicId);

            ClinicLocation? addedLocation = null;

            _clinicLocationRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<ClinicLocation>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ClinicLocation, CancellationToken>((location, _) =>
                {
                    addedLocation = location;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.AddAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(addedLocation);

            Assert.Equal(result.Value, addedLocation!.Id);
            Assert.Equal(request.ClinicId, addedLocation.ClinicId);
            Assert.Equal(request.Name, addedLocation.Name);
            Assert.Equal(request.AddressLine, addedLocation.AddressLine);
            Assert.Equal(request.City, addedLocation.City);
            Assert.Equal(request.Country, addedLocation.Country);
            Assert.Equal(request.PhoneNumber, addedLocation.PhoneNumber);
            Assert.Equal(request.TimeZoneId, addedLocation.TimeZoneId);
            Assert.True(addedLocation.IsActive);

            _clinicLocationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<ClinicLocation>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_WhenRepositoryThrows_PropagatesException()
        {
            var request = CreateRequest();

            SetupClinicExists(request.ClinicId);

            _clinicLocationRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<ClinicLocation>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _sut.AddAsync(
                    request,
                    CancellationToken.None));

            Assert.Equal("Database error", exception.Message);
        }

        [Fact]
        public async Task GetByIdResponseAsync_WhenClinicLocationDoesNotExist_ReturnsNotFound()
        {
            var id = Guid.NewGuid();

            _clinicLocationRepository
                .Setup(x => x.GetByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((ClinicLocation?)null);

            var result = await _sut.GetByIdResponseAsync(
                id,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
            Assert.Equal(
                "Clinic location not found.",
                result.Error);
        }

        [Fact]
        public async Task GetByIdResponseAsync_WhenClinicLocationExists_ReturnsMappedResponse()
        {
            var clinicLocation = CreateClinicLocation();

            _clinicLocationRepository
                .Setup(x => x.GetByIdAsync(
                    clinicLocation.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(clinicLocation);

            var result = await _sut.GetByIdResponseAsync(
                clinicLocation.Id,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(
                clinicLocation.ClinicId,
                result.Value.ClinicId);

            Assert.Equal(
                clinicLocation.Id,
                result.Value.ClinicLocationId);

            Assert.Equal(
                clinicLocation.Name,
                result.Value.Name);

            Assert.Equal(
                clinicLocation.AddressLine,
                result.Value.AddressLine);

            Assert.Equal(
                clinicLocation.City,
                result.Value.City);

            Assert.Equal(
                clinicLocation.Country,
                result.Value.Country);

            Assert.Equal(
                clinicLocation.PhoneNumber,
                result.Value.PhoneNumber);

            Assert.Equal(
                clinicLocation.TimeZoneId,
                result.Value.TimeZoneId);
        }

        [Fact]
        public async Task GetByIdResponseAsync_WhenRepositoryThrows_PropagatesException()
        {
            var id = Guid.NewGuid();

            _clinicLocationRepository
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
        private void SetupClinicExists(Guid clinicId)
        {
            _clinicRepository
                .Setup(x => x.GetByIdAsync(
                    clinicId,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Clinic
                {
                    Id = clinicId
                });
        }

        private void VerifyLocationWasNotAdded()
        {
            _clinicLocationRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<ClinicLocation>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static ClinicLocationRequest CreateRequest(Guid? clinicId = null,string? name = null,string? phoneNumber = null,string? timeZoneId = null,string? addressLine = null,string? city = null,string? country = null)
        {
            return new ClinicLocationRequest(
                ClinicId: clinicId ?? Guid.NewGuid(),
                Name: name ?? "Main Clinic",
                AddressLine: addressLine ?? "123 Main Street",
                City: city ?? "Cairo",
                Country: country ?? "Egypt",
                PhoneNumber: phoneNumber ?? "0220000000",
                TimeZoneId: timeZoneId ?? "Egypt Standard Time");
        }

        private static ClinicLocation CreateClinicLocation()
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
                TimeZoneId = "Egypt Standard Time",
                IsActive = true
            };
        }
    }
}
