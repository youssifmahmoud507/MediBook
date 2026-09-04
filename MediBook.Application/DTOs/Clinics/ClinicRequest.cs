using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Clinics
{
    public record ClinicRequest(string Name, string? Description, string PhoneNumber, string Email);
}
