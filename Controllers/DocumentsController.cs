using ClaimsAi.Api.Models;
using ClaimsAi.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsAi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController(DocumentExtractor extractor) : ControllerBase
    {

        // EXTRACTS DATA FROM MEDICAL BILL FOR ClaimDocument OBJECT USING SAVED PROMPT extract-bill.v1.txt
        [HttpPost("extract")]
        public async Task<IActionResult> Extract([FromBody] ExtractRequest req, CancellationToken ct)
        {
            try
            {
                var doc = await Retry.WithRetryAsync(() => extractor.ExtractAsync(req.Text, ct));
                if (doc is null)
                    return UnprocessableEntity(new { error = "Model output did not match schema" });

                var problems = DocumentExtractor.Validate(doc);
                return problems.Count == 0
                    ? Ok(doc)
                    : UnprocessableEntity(new { problems, doc });
            }
            catch (HttpRequestException)
            {
                return Problem("Model unavailable. Try again later.", statusCode: 503);
            }
        }
    }
}
