using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Notifications
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
