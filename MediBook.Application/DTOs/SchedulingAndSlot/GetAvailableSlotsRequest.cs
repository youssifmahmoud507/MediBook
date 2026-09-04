namespace MediBook.Application.DTOs.SchedulingAndSlot
{
    public record GetAvailableSlotsRequest(Guid ClinicLocationId, Guid AppointmentTypeId, DateOnly Date);
}
