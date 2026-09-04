using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Clinics
{
    public class ClinicResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string PhoneNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
    }
}
