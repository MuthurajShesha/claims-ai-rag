using ClaimsAi.Api.Models;
using ClaimsAi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsAi.Api.Controllers;

[ApiController]
[Route("coverage")]
public class CoverageController(CoverageService svc) : ControllerBase
{
    [HttpPost("ask")]
    public async Task<IActionResult> Ask([FromBody] CoverageRequest req, CancellationToken ct)
    {
        try
        {
            var answer = await Retry.WithRetryAsync(() => svc.AskAsync(req.Question, req.Plan, ct));
            return answer is null
                ? UnprocessableEntity(new { error = "No valid, cited answer could be produced." })
                : Ok(answer);
        }
        catch (HttpRequestException)
        {
            return Problem("Model unavailable. Try again later.", statusCode: 503);
        }
    }
}
