using MediBook.Application.DTOs.Notifications;
using MediBook.Application.Interfaces;
using MediBook.Application.Services;
using MediBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Infrustructure.Services
{
    public class NotificationService(INotificationRepository notificationRepository, IEmailSender emailSender) : INotificationService
    {
        private readonly INotificationRepository _notificationRepository = notificationRepository;
        private readonly IEmailSender _emailSender = emailSender;

        public async Task CreateNotificationAsync(Guid userId, string toEmail, string title, string message, CancellationToken cancellationToken)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            await _notificationRepository.AddAsync(notification, cancellationToken);
            await _emailSender.SendAsync(toEmail, title, message, cancellationToken);

        }

        public async Task<List<NotificationResponse>> GetMyNotificationsAsync(Guid userId, CancellationToken cancellationToken)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId, cancellationToken);
            return [.. notifications.Select(n => new NotificationResponse
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })];
        }
    }
}
