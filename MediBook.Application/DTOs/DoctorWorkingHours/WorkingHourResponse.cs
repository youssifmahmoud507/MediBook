using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace MediBook.Application.DTOs.DoctorWorkingHours
{
    public class WorkingHourResponse
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
