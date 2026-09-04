using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.ClinicLocations
{
    public class ClinicLocationResponse
    {
        public Guid ClinicLocationId { get; set; }
        public Guid ClinicId { get; set; }
        public string Name { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public string City { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public string TimeZoneId { get; set; } = default!;
    }
}
