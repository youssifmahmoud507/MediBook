using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class AttachmentsController(IAttachmentService attachmentService) : ApiBaseController
    {
        private readonly IAttachmentService _attachmentService = attachmentService;

        [Authorize]
        [HttpPost("~/api/medical-records/{medicalRecordId:guid}/attachments")]
        public async Task<IActionResult> UploadAttachment(Guid medicalRecordId, IFormFile file, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var result = await _attachmentService.UploadAttachmentAsync(medicalRecordId, file, currentUserId, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);
            return StatusCode(201, result.Value);
        }

        [Authorize]
        [HttpGet("{attachmentId:guid}/download")]
        public async Task<IActionResult> DownloadAttachment(Guid attachmentId, CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var currentUserId))
                return Unauthorized();

            var result = await _attachmentService.DownloadAttachmentAsync(attachmentId, currentUserId, cancellationToken);
            if (!result.IsSuccess) return HandleFailure(result);

            var (fileStream, contentType, fileName) = result.Value;
            return File(fileStream, contentType, fileName);
        }
    }
}
