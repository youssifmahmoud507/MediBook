using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class Patient : BaseEntity
    {
        public string FirstName { get;  set; } = default!;
        public string LastName { get;  set; } = default!;
        public DateOnly DateOfBirth { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string? Email { get; set; }
        public bool IsActive { get; set; } 
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? UserId { get; set; }
    }
}
