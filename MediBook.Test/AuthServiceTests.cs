using Microsoft.Extensions.Configuration;
using MediBook.Application.Common;
using MediBook.Application.DTOs.Auth;
using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Identity;
using MediBook.Infrustructure.Identity.Interfaces;
using MediBook.Infrustructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Test
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManager;
        private readonly Mock<IPatientRepository> _patientRepository;
        private readonly Mock<IDoctorRepository> _doctorRepository;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository;
        private readonly Mock<IConfiguration> _configuration;
        private readonly Mock<IUnitOfWork> _unitOfWork;

        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();

            _userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!);

            _patientRepository = new Mock<IPatientRepository>();
            _doctorRepository = new Mock<IDoctorRepository>();
            _jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
            _refreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _configuration = new Mock<IConfiguration>();
            _unitOfWork = new Mock<IUnitOfWork>();

            _sut = new AuthService(
                _userManager.Object,
                _patientRepository.Object,
                _doctorRepository.Object,
                _jwtTokenGenerator.Object,
                _refreshTokenRepository.Object,
                _configuration.Object,
                _unitOfWork.Object);
        }


        [Fact]
        public async Task RegisterDoctorAsync_WhenEmailIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateDoctorRequest(email: "");

            var result = await _sut.RegisterDoctorAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Email cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenPasswordIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateDoctorRequest(password: "");

            var result = await _sut.RegisterDoctorAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Password cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenFirstNameIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateDoctorRequest(firstName: "");

            var result = await _sut.RegisterDoctorAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("First name cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenLastNameIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateDoctorRequest(lastName: "");

            var result = await _sut.RegisterDoctorAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Last name cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenPhoneNumberIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateDoctorRequest(phoneNumber: "");

            var result = await _sut.RegisterDoctorAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Phone cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenLicenseNumberIsEmpty_ReturnsValidationFailure()
        {
            var request = CreateDoctorRequest(licenseNumber: "");

            var result = await _sut.RegisterDoctorAsync(request, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("License number cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenLicenseNumberAlreadyExists_ReturnsConflict()
        {
            var request = CreateDoctorRequest();

            _doctorRepository
                .Setup(x => x.ExistsWithLicenseNumberAsync(
                    request.LicenseNumber,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _sut.RegisterDoctorAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Conflict, result.ErrorType);
            Assert.Equal(
                "A doctor with the same license number already exists.",
                result.Error);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenUserCreationFails_RollsBackAndReturnsValidationFailure()
        {
            var request = CreateDoctorRequest();

            SetupDoctorLicenseNotExists(request);

            var errors = new[]
            {
            new IdentityError
            {
                Description = "Password is too weak."
            }
        };

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _sut.RegisterDoctorAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Password is too weak.", result.Error);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);

            _userManager.Verify(
                x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Doctor"),
                Times.Never);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenAddingRoleFails_RollsBackAndReturnsValidationFailure()
        {
            var request = CreateDoctorRequest();

            SetupDoctorLicenseNotExists(request);

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);

            var errors = new[]
            {
            new IdentityError
            {
                Description = "Role does not exist."
            }
        };

            _userManager
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Doctor"))
                .ReturnsAsync(IdentityResult.Failed(errors));

            var result = await _sut.RegisterDoctorAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Role does not exist.", result.Error);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);

            _doctorRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenSuccessful_CreatesDoctorAndCommitsTransaction()
        {
            var request = CreateDoctorRequest();

            SetupDoctorLicenseNotExists(request);

            ApplicationUser? createdUser = null;

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .Callback<ApplicationUser, string>((user, _) =>
                {
                    createdUser = user;
                })
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Doctor"))
                .ReturnsAsync(IdentityResult.Success);

            Doctor? createdDoctor = null;

            _doctorRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Doctor, CancellationToken>((doctor, _) =>
                {
                    createdDoctor = doctor;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.RegisterDoctorAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(createdUser);
            Assert.Equal(request.Email, createdUser!.Email);
            Assert.Equal(request.Email, createdUser.UserName);

            Assert.NotNull(createdDoctor);
            Assert.Equal(result.Value, createdDoctor!.Id);
            Assert.Equal(createdUser.Id, createdDoctor.UserId);
            Assert.Equal(request.FirstName, createdDoctor.FirstName);
            Assert.Equal(request.LastName, createdDoctor.LastName);
            Assert.Equal(request.PhoneNumber, createdDoctor.PhoneNumber);
            Assert.Equal(request.LicenseNumber, createdDoctor.LicenseNumber);
            Assert.Equal(request.Email, createdDoctor.Email);
            Assert.True(createdDoctor.IsActive);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterDoctorAsync_WhenRepositoryThrows_RollsBackAndReturnsFailure()
        {
            var request = CreateDoctorRequest();

            SetupDoctorLicenseNotExists(request);

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Doctor"))
                .ReturnsAsync(IdentityResult.Success);

            _doctorRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<Doctor>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _sut.RegisterDoctorAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Contains("An unexpected error occurred during registration.", result.Error);
            Assert.Contains("Database error", result.Error);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task RegisterPatientAsync_WhenEmailIsEmpty_ReturnsValidationFailure()
        {
            var request = CreatePatientRequest(email: "");

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Email cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenPasswordIsEmpty_ReturnsValidationFailure()
        {
            var request = CreatePatientRequest(password: "");

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Password cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenFirstNameIsEmpty_ReturnsValidationFailure()
        {
            var request = CreatePatientRequest(firstName: "");

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("First name cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenLastNameIsEmpty_ReturnsValidationFailure()
        {
            var request = CreatePatientRequest(lastName: "");

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Last name cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenPhoneNumberIsEmpty_ReturnsValidationFailure()
        {
            var request = CreatePatientRequest(phoneNumber: "");

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Phone cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenUserCreationFails_RollsBackAndReturnsValidationFailure()
        {
            var request = CreatePatientRequest();

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Email already exists."
                        }));

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Email already exists.", result.Error);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenAddingRoleFails_RollsBackAndReturnsValidationFailure()
        {
            var request = CreatePatientRequest();

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Patient"))
                .ReturnsAsync(
                    IdentityResult.Failed(
                        new IdentityError
                        {
                            Description = "Patient role does not exist."
                        }));

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Patient role does not exist.", result.Error);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);

            _patientRepository.Verify(
                x => x.CreatePatientAsync(
                    It.IsAny<Patient>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenSuccessful_CreatesPatientAndCommitsTransaction()
        {
            var request = CreatePatientRequest();

            ApplicationUser? createdUser = null;

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .Callback<ApplicationUser, string>((user, _) =>
                {
                    createdUser = user;
                })
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            Patient? createdPatient = null;

            _patientRepository
                .Setup(x => x.CreatePatientAsync(
                    It.IsAny<Patient>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Patient, CancellationToken>((patient, _) =>
                {
                    createdPatient = patient;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);

            Assert.NotNull(createdUser);
            Assert.Equal(request.Email, createdUser!.Email);
            Assert.Equal(request.Email, createdUser.UserName);

            Assert.NotNull(createdPatient);
            Assert.Equal(result.Value, createdPatient!.Id);
            Assert.Equal(createdUser.Id, createdPatient.UserId);
            Assert.Equal(request.FirstName, createdPatient.FirstName);
            Assert.Equal(request.LastName, createdPatient.LastName);
            Assert.Equal(request.PhoneNumber, createdPatient.PhoneNumber);
            Assert.Equal(request.Email, createdPatient.Email);
            Assert.Equal(request.DateOfBirth, createdPatient.DateOfBirth);
            Assert.True(createdPatient.IsActive);

            _unitOfWork.Verify(
                x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterPatientAsync_WhenRepositoryThrows_RollsBackAndReturnsFailure()
        {
            var request = CreatePatientRequest();

            _userManager
                .Setup(x => x.CreateAsync(
                    It.IsAny<ApplicationUser>(),
                    request.Password))
                .ReturnsAsync(IdentityResult.Success);

            _userManager
                .Setup(x => x.AddToRoleAsync(
                    It.IsAny<ApplicationUser>(),
                    "Patient"))
                .ReturnsAsync(IdentityResult.Success);

            _patientRepository
                .Setup(x => x.CreatePatientAsync(
                    It.IsAny<Patient>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _sut.RegisterPatientAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Contains("An unexpected error occurred during registration.", result.Error);
            Assert.Contains("Database error", result.Error);

            _unitOfWork.Verify(
                x => x.RollbackTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Once);

            _unitOfWork.Verify(
                x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        }


        [Fact]
        public async Task LoginAsync_WhenEmailIsEmpty_ReturnsValidationFailure()
        {
            var request = new LoginRequest("", "Password123");

            var result = await _sut.LoginAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Email cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsEmpty_ReturnsValidationFailure()
        {
            var request = new LoginRequest(
                "doctor@test.com",
                "");

            var result = await _sut.LoginAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Password cannot be null or empty.", result.Error);
        }

        [Fact]
        public async Task LoginAsync_WhenUserDoesNotExist_ReturnsValidationFailure()
        {
            var request = new LoginRequest(
                "doctor@test.com",
                "Password123");

            _userManager
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.LoginAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid email or password.", result.Error);
        }

        [Fact]
        public async Task LoginAsync_WhenPasswordIsInvalid_ReturnsValidationFailure()
        {
            var request = new LoginRequest(
                "doctor@test.com",
                "WrongPassword");

            var user = CreateUser(request.Email);

            _userManager
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManager
                .Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

            var result = await _sut.LoginAsync(
                request,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid email or password.", result.Error);

            _userManager.Verify(
                x => x.GetRolesAsync(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAuthResponse()
        {
            var request = new LoginRequest(
                "doctor@test.com",
                "Password123");

            var user = CreateUser(request.Email);

            var roles = new List<string>
        {
            "Doctor"
        };

            const string accessToken = "access-token";

            _userManager
                .Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _userManager
                .Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _jwtTokenGenerator
                .Setup(x => x.GenerateToken(user, roles))
                .Returns(accessToken);

            _configuration
                .Setup(x => x["Jwt:ExpiryMinutes"])
                .Returns("30");

            RefreshToken? createdRefreshToken = null;

            _refreshTokenRepository
                .Setup(x => x.AddAsync(
                    It.IsAny<RefreshToken>(),
                    It.IsAny<CancellationToken>()))
                .Callback<RefreshToken, CancellationToken>((token, _) =>
                {
                    createdRefreshToken = token;
                })
                .Returns(Task.CompletedTask);

            var result = await _sut.LoginAsync(
                request,
                CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(accessToken, result.Value.AccessToken);
            Assert.False(string.IsNullOrWhiteSpace(result.Value.RefreshToken));
            Assert.True(result.Value.ExpiresAt > DateTimeOffset.UtcNow);

            Assert.NotNull(createdRefreshToken);
            Assert.Equal(user.Id, createdRefreshToken!.UserId);
            Assert.Equal(result.Value.RefreshToken, createdRefreshToken.Token);
            Assert.False(createdRefreshToken.IsRevoked);
            Assert.True(createdRefreshToken.ExpiresAt > DateTimeOffset.UtcNow);

            _jwtTokenGenerator.Verify(
                x => x.GenerateToken(user, roles),
                Times.Once);

            _refreshTokenRepository.Verify(
                x => x.AddAsync(
                    It.IsAny<RefreshToken>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }


        [Fact]
        public async Task RefreshTokenAsync_WhenTokenDoesNotExist_ReturnsValidationFailure()
        {
            const string refreshToken = "invalid-token";

            _refreshTokenRepository
                .Setup(x => x.GetByTokenAsync(
                    refreshToken,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((RefreshToken?)null);

            var result = await _sut.RefreshTokenAsync(
                refreshToken,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid refresh token.", result.Error);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenIsRevoked_ReturnsValidationFailure()
        {
            const string refreshToken = "revoked-token";

            var token = CreateRefreshToken(
                refreshToken,
                isRevoked: true);

            _refreshTokenRepository
                .Setup(x => x.GetByTokenAsync(
                    refreshToken,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var result = await _sut.RefreshTokenAsync(
                refreshToken,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid refresh token.", result.Error);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenIsExpired_ReturnsValidationFailure()
        {
            const string refreshToken = "expired-token";

            var token = CreateRefreshToken(
                refreshToken,
                expiresAt: DateTimeOffset.UtcNow.AddMinutes(-1));

            _refreshTokenRepository
                .Setup(x => x.GetByTokenAsync(
                    refreshToken,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            var result = await _sut.RefreshTokenAsync(
                refreshToken,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid refresh token.", result.Error);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenUserDoesNotExist_ReturnsValidationFailure()
        {
            const string refreshToken = "valid-token";

            var token = CreateRefreshToken(refreshToken);

            _refreshTokenRepository
                .Setup(x => x.GetByTokenAsync(
                    refreshToken,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _userManager
                .Setup(x => x.FindByIdAsync(token.UserId.ToString()))
                .ReturnsAsync((ApplicationUser?)null);

            var result = await _sut.RefreshTokenAsync(
                refreshToken,
                CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(ErrorType.Validation, result.ErrorType);
            Assert.Equal("Invalid refresh token.", result.Error);
        }

        [Fact]
        public async Task RefreshTokenAsync_WhenTokenAndUserAreValid_ReturnsNewAccessToken()
        {
            const string refreshToken = "valid-refresh-token";

            var token = CreateRefreshToken(refreshToken);

            var user = CreateUser("doctor@test.com");

            var roles = new List<string>
        {
            "Doctor"
        };

            const string newAccessToken = "new-access-token";

            _refreshTokenRepository
                .Setup(x => x.GetByTokenAsync(
                    refreshToken,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(token);

            _userManager
                .Setup(x => x.FindByIdAsync(token.UserId.ToString()))
                .ReturnsAsync(user);

            _userManager
                .Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(roles);

            _jwtTokenGenerator
                .Setup(x => x.GenerateToken(user, roles))
                .Returns(newAccessToken);

            _configuration
                .Setup(x => x["Jwt:ExpiryMinutes"])
                .Returns("30");

            var before = DateTimeOffset.UtcNow;

            var result = await _sut.RefreshTokenAsync(
                refreshToken,
                CancellationToken.None);

            var after = DateTimeOffset.UtcNow;

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(newAccessToken, result.Value.AccessToken);
            Assert.Equal(refreshToken, result.Value.RefreshToken);

            Assert.InRange(
                result.Value.ExpiresAt,
                before.AddMinutes(30),
                after.AddMinutes(30));

            _jwtTokenGenerator.Verify(
                x => x.GenerateToken(user, roles),
                Times.Once);
        }

        private void SetupDoctorLicenseNotExists(RegisterDoctorRequest request)
        {
            _doctorRepository
                .Setup(x => x.ExistsWithLicenseNumberAsync(
                    request.LicenseNumber,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private static ApplicationUser CreateUser(string email)
        {
            return new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = email
            };
        }

        private static RefreshToken CreateRefreshToken(string token,bool isRevoked = false,DateTimeOffset? expiresAt = null)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Token = token,
                ExpiresAt = expiresAt ?? DateTimeOffset.UtcNow.AddDays(7),
                IsRevoked = isRevoked,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        private static RegisterDoctorRequest CreateDoctorRequest(string? email = null,string? password = null,string? firstName = null,string? lastName = null,string? phoneNumber = null,string? licenseNumber = null)
        {
            return new RegisterDoctorRequest(
                Email: email ?? "doctor@test.com",
                Password: password ?? "Password123!",
                FirstName: firstName ?? "John",
                LastName: lastName ?? "Doe",
                PhoneNumber: phoneNumber ?? "01000000000",
                LicenseNumber: licenseNumber ?? "LIC-001");
        }

        private static RegisterPatientRequest CreatePatientRequest(string? email = null,string? password = null,string? firstName = null,string? lastName = null,string? phoneNumber = null)
        {
            return new RegisterPatientRequest(
                Email: email ?? "patient@test.com",
                Password: password ?? "Password123!",
                FirstName: firstName ?? "Jane",
                LastName: lastName ?? "Doe",
                PhoneNumber: phoneNumber ?? "01100000000",
                DateOfBirth: new DateOnly(1995, 1, 1));
        }
    }
}
