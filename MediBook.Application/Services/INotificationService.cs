using MediBook.Application.DTOs.Notifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(Guid userId, string toEmail, string title, string message, CancellationToken cancellationToken);
        Task<List<NotificationResponse>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
