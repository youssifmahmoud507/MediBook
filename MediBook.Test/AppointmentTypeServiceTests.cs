using MediBook.Application.Common;
using MediBook.Application.DTOs.AppointmentTypes;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class AppointmentTypeServiceTests
    {
        private readonly Mock<IAppointmentTypeRepository> _repositoryMock;
        private readonly AppointmentTypeService _sut;

        public AppointmentTypeServiceTests()
        {
            _repositoryMock = new Mock<IAppointmentTypeRepository>();

            _sut = new AppointmentTypeService(_repositoryMock.Object);
        }

        [Fact]
        public async Task CreateAppointmentTypeAsync_NameIsEmpty_ReturnsValidation()
        {
            // Arrange
            var request = new CreateAppointmentTypeRequest(
                Name: "",
                Description: "Test Description",
                DurationMinutes: 30);

            // Act
            var result = await _sut.CreateAppointmentTypeAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);

            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.IsAny<AppointmentType>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentTypeAsync_NameIsWhiteSpace_ReturnsValidation()
        {
            // Arrange
            var request = new CreateAppointmentTypeRequest(
                Name: "   ",
                Description: "Test Description",
                DurationMinutes: 30);

            // Act
            var result = await _sut.CreateAppointmentTypeAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);

            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.IsAny<AppointmentType>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentTypeAsync_DurationIsZero_ReturnsValidation()
        {
            // Arrange
            var request = new CreateAppointmentTypeRequest(
                Name: "Consultation",
                Description: "Test Description",
                DurationMinutes: 0);

            // Act
            var result = await _sut.CreateAppointmentTypeAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);

            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.IsAny<AppointmentType>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentTypeAsync_DurationIsNegative_ReturnsValidation()
        {
            // Arrange
            var request = new CreateAppointmentTypeRequest(
                Name: "Consultation",
                Description: "Test Description",
                DurationMinutes: -30);

            // Act
            var result = await _sut.CreateAppointmentTypeAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);

            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.IsAny<AppointmentType>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task CreateAppointmentTypeAsync_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var request = new CreateAppointmentTypeRequest(
                Name: "Consultation",
                Description: "General doctor consultation",
                DurationMinutes: 30);

            // Act
            var result = await _sut.CreateAppointmentTypeAsync(
                request,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            _repositoryMock.Verify(
                r => r.AddAsync(
                    It.Is<AppointmentType>(appointmentType =>
                        appointmentType.Id == result.Value &&
                        appointmentType.Name == request.Name &&
                        appointmentType.Description == request.Description &&
                        appointmentType.DurationMinutes == request.DurationMinutes &&
                        appointmentType.IsActive == true),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task GetByIdAsync_AppointmentTypeDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((AppointmentType?)null);

            // Act
            var result = await _sut.GetByIdAsync(
                id,
                CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.NotFound, result.ErrorType);
        }


        [Fact]
        public async Task GetByIdAsync_AppointmentTypeExists_ReturnsSuccessWithCorrectData()
        {
            // Arrange
            var appointmentType = new AppointmentType
            {
                Id = Guid.NewGuid(),
                Name = "Consultation",
                Description = "General doctor consultation",
                DurationMinutes = 30,
                IsActive = true
            };

            _repositoryMock
                .Setup(r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(appointmentType);

            // Act
            var result = await _sut.GetByIdAsync(
                appointmentType.Id,
                CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.NotNull(result.Value);

            Assert.Equal(
                appointmentType.Id,
                result.Value.Id);

            Assert.Equal(
                appointmentType.Name,
                result.Value.Name);

            Assert.Equal(
                appointmentType.Description,
                result.Value.Description);

            Assert.Equal(
                appointmentType.DurationMinutes,
                result.Value.DurationMinutes);

            Assert.Equal(
                appointmentType.IsActive,
                result.Value.IsActive);

            _repositoryMock.Verify(
                r => r.GetByIdAsync(
                    appointmentType.Id,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
