using ClaimsAi.Api.Models;
using ClaimsAi.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace ClaimsAi.Api.Controllers;

[ApiController]
[Route("assistant")]
public class AssistantController(IChatClient chat, ClaimTools tools) : ControllerBase
{
    // Tool-calling loop (policy data + RAG clause search) is run by UseFunctionInvocation().
    [HttpPost]
    public async Task<IActionResult> Ask([FromBody] ChatRequest req, CancellationToken ct)
    {
        var options = new ChatOptions { Temperature = 0f, Tools = tools.AsTools() };
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, PromptStore.Load("claims-assistant.v1.txt")),
            new(ChatRole.User, req.Message)
        };
        var response = await chat.GetResponseAsync(messages, options, ct);
        return Ok(new { reply = response.Text });
    }
}