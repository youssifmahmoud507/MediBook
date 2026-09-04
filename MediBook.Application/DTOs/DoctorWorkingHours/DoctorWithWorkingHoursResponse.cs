namespace MediBook.Application.DTOs.DoctorWorkingHours
{
    public class DoctorWithWorkingHoursResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string LicenseNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public List<WorkingHourResponse> WorkingHours { get; set; } = [];
    }
}
