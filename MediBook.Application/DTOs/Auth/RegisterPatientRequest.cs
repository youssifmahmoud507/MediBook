using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Auth
{
    public record RegisterPatientRequest(string Email,string Password,string FirstName,string LastName,DateOnly DateOfBirth,string PhoneNumber);
}
