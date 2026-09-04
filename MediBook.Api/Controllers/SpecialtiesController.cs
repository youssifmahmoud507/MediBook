using MediBook.Application.DTOs.Specialities;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    public class SpecialtiesController(ISpecialtyService specialtyService) : ApiBaseController
    {
        private readonly ISpecialtyService _specialtyService = specialtyService;

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecialtyAsync(Guid id, CancellationToken cancellationToken)
        {
            var result = await _specialtyService.GetSpecialtyAsync(id, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddSpecialtyAsync(SpecialityRequest request, CancellationToken cancellationToken)
        {
            var result = await _specialtyService.AddSpecialtyAsync(request, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }


    }
}
