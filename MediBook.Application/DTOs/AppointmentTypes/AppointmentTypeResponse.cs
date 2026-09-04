namespace MediBook.Application.DTOs.AppointmentTypes
{
    public class AppointmentTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }
}
