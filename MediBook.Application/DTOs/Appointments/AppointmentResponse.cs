using MediBook.Domain.Enums;

namespace MediBook.Application.DTOs.Appointments
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid ClinicLocationId { get; set; }
        public Guid AppointmentTypeId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

    }
}
