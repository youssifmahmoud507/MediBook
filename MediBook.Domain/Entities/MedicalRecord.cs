using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class MedicalRecord : BaseEntity
    {
        public Guid AppointmentId { get; set; }
        public string Diagnosis { get; set; } = default!;
        public string? Notes { get; set; }
        public string? TreatmentPlan { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
