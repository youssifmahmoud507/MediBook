namespace MediBook.Application.DTOs.Auth
{
    public record RegisterDoctorRequest(string Email,string Password,string FirstName,string LastName,string PhoneNumber,string LicenseNumber);
}
