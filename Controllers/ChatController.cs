using ClaimsAi.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace ClaimsAi.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController(IChatClient chat) : ControllerBase
    {

        // It sends reply at once
        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest req, CancellationToken ct)
        {
            var response = await chat.GetResponseAsync(req.Message, cancellationToken: ct);
            return Ok(new
            {
                reply = response.Text,
                inputTokens = response.Usage?.InputTokenCount,
                outputTokens = response.Usage?.OutputTokenCount
            });
        }



        // replies response chunk by chunk, which means it streams result like chatgpt loading result
        [HttpPost("stream")]
        public IAsyncEnumerable<string> StreamChat([FromBody] ChatRequest req, CancellationToken ct)
      => StreamInternal(req.Message, ct);

        private async IAsyncEnumerable<string> StreamInternal(string message,
            [EnumeratorCancellation] CancellationToken ct)
        {
            await foreach (var update in chat.GetStreamingResponseAsync(message, cancellationToken: ct))
                if (!string.IsNullOrEmpty(update.Text)) yield return update.Text;
        }
    }
}
