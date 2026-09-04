using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class Clinic : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
