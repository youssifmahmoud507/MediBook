using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Patients
{
    public record CreatePatientRequest(string FirstName,string LastName,DateOnly DateOfBirth,string PhoneNumber,string? Email);
}
