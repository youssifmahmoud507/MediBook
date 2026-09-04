using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.DoctorWorkingHours
{
    public record AddWorkingHourRequest(DayOfWeek DayOfWeek, TimeOnly StartTime, TimeOnly EndTime);


}
