using System;
using System.Collections.Generic;
using System.Text;

namespace MediBook.Application.DTOs.MedidcalRecords
{
    public record CreateMedicalRecordRequest(string Diagnosis, string? Notes, string? TreatmentPlan);
}
