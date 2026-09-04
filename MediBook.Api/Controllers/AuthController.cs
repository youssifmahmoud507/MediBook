using MediBook.Application.DTOs.Auth;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    [AllowAnonymous]
    public class AuthController(IAuthService authService) : ApiBaseController
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register/patient")]
        public async Task<IActionResult> RegisterPatient(RegisterPatientRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RegisterPatientAsync(request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return StatusCode(201, result.Value);
        }

        [HttpPost("register/doctor")]
        public async Task<IActionResult> RegisterDoctor(RegisterDoctorRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RegisterDoctorAsync(request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return StatusCode(201, result.Value);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.LoginAsync(request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }
        
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }
    }
}
