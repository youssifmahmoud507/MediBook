using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.ClinicLocations
{
    public record ClinicLocationRequest(Guid ClinicId, string Name,string AddressLine,string City,string Country,string PhoneNumber,string TimeZoneId);
}
