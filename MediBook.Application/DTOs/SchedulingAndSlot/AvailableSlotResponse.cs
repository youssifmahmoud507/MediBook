using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.SchedulingAndSlot
{
    public class AvailableSlotResponse
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
