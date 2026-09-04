using MediBook.Application.DTOs.Appointments;
using MediBook.Application.DTOs.SchedulingAndSlot;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController(IAppointmentService appointmentService) : ApiBaseController
    {
        private readonly IAppointmentService _appointmentService = appointmentService;

        [Authorize(Roles = "Patient,Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAppointment(CreateAppointmentRequest request, CancellationToken cancellationToken)
        {
            var result = await _appointmentService.CreateAppointmentAsync(request, cancellationToken);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Value });


        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetAppointmentById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _appointmentService.GetAppointmentByIdAsync(
                id,
                cancellationToken);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(result.Value);
        }

        [Authorize]
        [HttpPost("{id:guid}/cancel")]
        public async Task<IActionResult> CancelAppointment(Guid id, CancellationToken cancellationToken)
        {
            var result = await _appointmentService.CancelAppointmentAsync(id, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("~/api/doctors/{doctorId:guid}/available-slots")]
        public async Task<IActionResult> GetAvailableSlots(Guid doctorId,[FromQuery] GetAvailableSlotsRequest request,CancellationToken cancellationToken)
        {
            var result = await _appointmentService.GetAvailableSlotsAsync(doctorId, request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }
    }
}
