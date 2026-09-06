using MediBook.Application.Interfaces;
using MediBook.Domain.Entities;
using MediBook.Infrustructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Repositories
{
    public class NotificationRepository(MediBookDbContext context) : INotificationRepository
    {
        private readonly MediBookDbContext _context = context;

        public async Task AddAsync(Notification notification, CancellationToken cancellationToken)
        {
            await _context.Notifications.AddAsync(notification, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _context.Notifications.Where(n => n.UserId == userId).OrderByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);
        }
    }
}
