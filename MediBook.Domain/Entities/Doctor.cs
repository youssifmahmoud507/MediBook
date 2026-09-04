using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string LicenseNumber { get; set; } = default!;
        public bool IsActive { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? UserId { get; set; }


    }
}
