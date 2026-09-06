using MediBook.Application.DTOs.MedidcalRecords;
using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalRecordsController(IMedicalRecordService medicalRecordService) : ApiBaseController
    {
        private readonly IMedicalRecordService _medicalRecordService = medicalRecordService;

        //POST /api/appointments/{appointmentId}/medical-record — [Authorize], extract currentUserId from claims, call CreateMedicalRecordAsync.

        [Authorize]
        [HttpPost("/api/appointments/{appointmentId}/medical-record")]
        public async Task<IActionResult> CreateMedicalRecord(Guid appointmentId, CreateMedicalRecordRequest request, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var result = await _medicalRecordService.CreateMedicalRecordAsync(appointmentId, request, currentUserId, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return CreatedAtAction(nameof(GetMedicalRecordById), new { appointmentId }, null);
        }

        [Authorize]
        [HttpGet("/api/appointments/{appointmentId}/medical-record")]
        public async Task<IActionResult> GetMedicalRecordById(Guid appointmentId, CancellationToken cancellationToken = default)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var result = await _medicalRecordService.GetMedicalRecordAsync(appointmentId, currentUserId, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return Ok(result.Value);
        }
    }
}
