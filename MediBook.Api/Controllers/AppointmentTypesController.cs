using MediBook.Application.DTOs.AppointmentTypes;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    public class AppointmentTypesController(IAppointmentTypeService service) : ApiBaseController
    {
        private readonly IAppointmentTypeService _service = service;

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAppointmentTypeRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAppointmentTypeAsync(request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value }, null);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }
    }
}
