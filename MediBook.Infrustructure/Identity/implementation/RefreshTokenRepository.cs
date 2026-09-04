using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using MediBook.Infrustructure.Identity.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Identity.implementation
{
    public class RefreshTokenRepository(MediBookDbContext context) : IRefreshTokenRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
        {
             _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
