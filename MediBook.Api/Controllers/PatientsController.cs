using MediBook.Application.DTOs.Patients;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    public class PatientsController(IPatientService patientService) : ApiBaseController
    {
        private readonly IPatientService _patientService = patientService;

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePatientAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _patientService.CreatePatientAsync(request, cancellationToken);
            if (result.IsSuccess) return StatusCode(201 , result.Value);
            return HandleFailure(result);
        }
    }
}
