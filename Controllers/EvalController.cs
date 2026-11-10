using System.Text.Json;
using ClaimsAi.Api.Models;
using ClaimsAi.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;

namespace ClaimsAi.Api.Controllers;

[ApiController]
[Route("eval")]
public class EvalController(
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    InMemoryVectorStore store) : ControllerBase
{
    // recall@k: how often does the expected clause appear in the top k results?
    [HttpGet("retrieval")]
    public async Task<IActionResult> Retrieval([FromQuery] int k = 4, CancellationToken ct = default)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "retrieval-tests.json");
        var tests = JsonSerializer.Deserialize<List<RetrievalTest>>(
            await System.IO.File.ReadAllTextAsync(path, ct),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        var misses = new List<object>();
        var hit = 0;
        foreach (var t in tests)
        {
            var q = await embedder.GenerateAsync([t.Question], cancellationToken: ct);
            var top = store.Search(q[0].Vector.ToArray(), k, t.Plan);
            if (top.Any(x => x.Chunk.Clause == t.ExpectedClause)) hit++;
            else misses.Add(new { t.Question, t.ExpectedClause, got = top.Select(x => x.Chunk.Clause) });
        }

        return Ok(new { k, hits = hit, total = tests.Count, recall = Math.Round((double)hit / tests.Count, 3), misses });
    }
}
