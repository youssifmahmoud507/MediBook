using MediBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Domain.Entities
{
    public class ClinicLocation : BaseEntity
    {
        public Guid ClinicId { get; set; }
        public Clinic Clinic { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public string City { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string TimeZoneId { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
