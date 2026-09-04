using MediBook.Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MediBook.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class ApiBaseController : ControllerBase
    {
        protected IActionResult HandleFailure(Result result) => result.ErrorType switch
        {
            ErrorType.Validation => BadRequest(result.Error),
            ErrorType.NotFound => NotFound(result.Error),
            ErrorType.Conflict => Conflict(result.Error),
            _ => BadRequest(result.Error)
        };
    }
}
