using MediBook.Domain.Common;
using MediBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; } = default!;
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = default!;
        public Guid AppointmentTypeId { get; set; }
        public AppointmentType AppointmentType { get; set; } = default!;
        public Guid ClinicLocationId { get; set; }
        public ClinicLocation ClinicLocation { get; set; } = default!;


        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }

        public void Cancel(DateTimeOffset cancelledAt)
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new InvalidOperationException("Appointment is already cancelled.");

            Status = AppointmentStatus.Cancelled;
            CancelledAt = cancelledAt;
        }
    }
}
