using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class DoctorSpecialization
    {
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = default!;
        public Guid SpecializationId { get; set; }
        public Specialty Specialization { get; set; } = default!;
        public bool IsPrimary { get; set; }
    }
}
