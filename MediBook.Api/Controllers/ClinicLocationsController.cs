using MediBook.Application.DTOs.ClinicLocations;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    public class ClinicLocationsController(IClinicLocationService clinicLocationService) : ApiBaseController
    {
        private readonly IClinicLocationService _clinicLocationService = clinicLocationService;

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddAsync([FromBody] ClinicLocationRequest request, CancellationToken cancellationToken)
        {
            var result = await _clinicLocationService.AddAsync(request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _clinicLocationService.GetByIdResponseAsync(id, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }
    }
}
