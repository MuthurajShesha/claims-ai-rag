using ClaimsAi.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClaimsAi.Api.Controllers;

[ApiController]
public class HomeController(InMemoryVectorStore store) : ControllerBase
{
    [HttpGet("/")]
    public IActionResult Index() => Ok("ClaimsAi API is running");

    [HttpGet("index/status")]
    public IActionResult Status() => Ok(new { indexedClauses = store.Count });
}
