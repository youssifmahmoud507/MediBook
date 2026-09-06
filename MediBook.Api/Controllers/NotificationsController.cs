using MediBook.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class NotificationsController(INotificationService notificationService) : ApiBaseController
    {
        private readonly INotificationService _notificationService = notificationService;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var notifications = await _notificationService.GetMyNotificationsAsync(userId, cancellationToken);
            return Ok(notifications);
        }
    }
}
