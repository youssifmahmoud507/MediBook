namespace MediBook.Application.DTOs.MedidcalRecords
{
    public class MedicalRecordResponse
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public string Diagnosis { get; set; } = default!;
        public string? Notes { get; set; }
        public string? TreatmentPlan { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
