using MediBook.Application.DTOs.Clinics;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    public class ClinicsController(IClinicService clinicService) : ApiBaseController
    {
        private readonly IClinicService _clinicService = clinicService;

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddClinicAsync(ClinicRequest request , CancellationToken cancellationToken = default)
        {
            var result = await _clinicService.AddAsync(request, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return StatusCode(201, result.Value);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetClinicByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _clinicService.GetByIdResponseAsync(id, cancellationToken);
            if (!result.IsSuccess)
                return HandleFailure(result);
            return Ok(result.Value);
        }
    }
}
