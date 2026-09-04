using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Doctors
{
    public record DoctorRequest(string FirstName, string LastName, string LicenseNumber, string Email, string PhoneNumber);
}
