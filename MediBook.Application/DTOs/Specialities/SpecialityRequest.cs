using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Specialities
{
    public record SpecialityRequest(string Name, string? Description);
}
