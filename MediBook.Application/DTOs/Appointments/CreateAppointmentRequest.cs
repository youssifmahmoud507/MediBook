using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.Appointments
{
    public record CreateAppointmentRequest(Guid PatientId,Guid DoctorId,Guid ClinicLocationId,Guid AppointmentTypeId,DateTimeOffset StartTime);
}
