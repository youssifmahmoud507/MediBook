using MediBook.Application.Common;
using MediBook.Application.DTOs.Auth;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Identity;
using MediBook.Infrustructure.Identity.implementation;
using MediBook.Infrustructure.Identity.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class AuthService(UserManager<ApplicationUser> userManager, IPatientRepository patientRepository, IDoctorRepository doctorRepository , IJwtTokenGenerator jwtTokenGenerator , IRefreshTokenRepository refreshTokenRepository , IConfiguration configuration , IUnitOfWork unitOfWork) : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IPatientRepository _patientRepository = patientRepository;
        private readonly IDoctorRepository _doctorRepository = doctorRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
        private readonly IConfiguration _configuration = configuration;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<Result<Guid>> RegisterDoctorAsync(RegisterDoctorRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email))
                return Result<Guid>.Failure("Email cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.Password))
                return Result<Guid>.Failure("Password cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.FirstName))
                return Result<Guid>.Failure("First name cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.LastName))
                return Result<Guid>.Failure("Last name cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.PhoneNumber))
                return Result<Guid>.Failure("Phone cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.LicenseNumber))
                return Result<Guid>.Failure("License number cannot be null or empty.", ErrorType.Validation);

            var licenseNumberExists = await _doctorRepository.ExistsWithLicenseNumberAsync(request.LicenseNumber, cancellationToken);
            if (licenseNumberExists)
                return Result<Guid>.Failure("A doctor with the same license number already exists.", ErrorType.Conflict);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Email,
                    Email = request.Email
                };

                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    var errorMessage = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return Result<Guid>.Failure(errorMessage, ErrorType.Validation);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Doctor");
                if (!roleResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    var errorMessage = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    return Result<Guid>.Failure(errorMessage, ErrorType.Validation);
                }

                var doctor = new Doctor
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    LicenseNumber = request.LicenseNumber,
                    Email = request.Email,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _doctorRepository.AddAsync(doctor, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return Result<Guid>.Success(doctor.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Guid>.Failure($"An unexpected error occurred during registration. {ex}", ErrorType.Validation);
            }
        }

        public async Task<Result<Guid>> RegisterPatientAsync(RegisterPatientRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email))
                return Result<Guid>.Failure("Email cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.Password))
                return Result<Guid>.Failure("Password cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.FirstName))
                return Result<Guid>.Failure("First name cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.LastName))
                return Result<Guid>.Failure("Last name cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.PhoneNumber))
                return Result<Guid>.Failure("Phone cannot be null or empty.", ErrorType.Validation);

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email
            };
            
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                var createResult = await _userManager.CreateAsync(user, request.Password);
                if (!createResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    var errorMessage = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    return Result<Guid>.Failure(errorMessage, ErrorType.Validation);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, "Patient");
                if (!roleResult.Succeeded)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    var errorMessage = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                    return Result<Guid>.Failure(errorMessage, ErrorType.Validation);
                }

                var patient = new Patient
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    DateOfBirth = request.DateOfBirth,
                    Email = request.Email,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsActive = true
                };

                await _patientRepository.CreatePatientAsync(patient, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return Result<Guid>.Success(patient.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result<Guid>.Failure($"An unexpected error occurred during registration. {ex}", ErrorType.Validation);
            }
        }

        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Email))
                return Result<AuthResponse>.Failure("Email cannot be null or empty.", ErrorType.Validation);
            if (string.IsNullOrEmpty(request.Password))
                return Result<AuthResponse>.Failure("Password cannot be null or empty.", ErrorType.Validation);

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
                return Result<AuthResponse>.Failure("Invalid email or password.", ErrorType.Validation);

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
                return Result<AuthResponse>.Failure("Invalid email or password.", ErrorType.Validation);


            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenGenerator.GenerateToken(user, roles);

            var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshTokenValue,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

            var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"]!);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshTokenValue,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30) // should match Jwt:ExpiryMinutes from config
            });
        }

        public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
        {
            var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken , cancellationToken);
            if (existingToken == null)
            {
                return Result<AuthResponse>.Failure("Invalid refresh token.", ErrorType.Validation);
            }

            if (existingToken.IsRevoked)
            {
                return Result<AuthResponse>.Failure("Invalid refresh token.", ErrorType.Validation);
            }

            if (existingToken.ExpiresAt < DateTimeOffset.UtcNow)
            {
                return Result<AuthResponse>.Failure("Invalid refresh token.", ErrorType.Validation);
            }

            var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
            if (user == null)
            {
                return Result<AuthResponse>.Failure("Invalid refresh token.", ErrorType.Validation);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _jwtTokenGenerator.GenerateToken(user, roles);

            var expiryMinutes = double.Parse(_configuration["Jwt:ExpiryMinutes"]!);

            return Result<AuthResponse>.Success(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes)
            });
        }
    }
}