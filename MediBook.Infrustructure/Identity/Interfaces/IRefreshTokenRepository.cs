using MediBook.Domain.Entities;

namespace MediBook.Infrustructure.Identity.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
        Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    }
}
