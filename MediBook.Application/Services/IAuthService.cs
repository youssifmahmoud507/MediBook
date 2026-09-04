using MediBook.Application.Common;
using MediBook.Application.DTOs.Auth;

namespace MediBook.Application.Services
{
    public interface IAuthService
    {
        Task<Result<Guid>> RegisterPatientAsync(RegisterPatientRequest request, CancellationToken cancellationToken);
        Task<Result<Guid>> RegisterDoctorAsync(RegisterDoctorRequest request, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
        Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    }
}
