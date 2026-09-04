using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class DoctorClinicAssignment
    {
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = default!;
        public Guid ClinicLocationId { get; set; }
        public ClinicLocation ClinicLocation { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
