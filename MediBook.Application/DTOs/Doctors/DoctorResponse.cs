using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Doctors
{
    public class DoctorResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string LicenseNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
    }
}
