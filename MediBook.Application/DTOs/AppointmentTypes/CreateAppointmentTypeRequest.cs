using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.AppointmentTypes
{
    public record CreateAppointmentTypeRequest(string Name, string? Description, int DurationMinutes);
}
