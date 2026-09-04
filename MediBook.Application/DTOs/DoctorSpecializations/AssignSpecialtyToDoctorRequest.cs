using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.DoctorSpecializations
{
    public record AssignSpecialtyToDoctorRequest(Guid SpecialtyId, bool IsPrimary);
}
