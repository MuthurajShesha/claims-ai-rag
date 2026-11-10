using ClaimsAi.Api.Models;
using Microsoft.Extensions.AI;

namespace ClaimsAi.Api.Services;

public class CoverageService(
    IChatClient chat,
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    InMemoryVectorStore store)
{
    public async Task<CoverageAnswer?> AskAsync(string question, string plan, CancellationToken ct)
    {
        var q = await embedder.GenerateAsync([question], cancellationToken: ct);
        var hits = store.Search(q[0].Vector.ToArray(), topK: 4, plan: plan);

        // Never answer without retrieved evidence.
        if (hits.Count == 0)
            return new CoverageAnswer($"No policy text found for plan '{plan}'.", [], false);

        var context = string.Join("\n\n", hits.Select(h => $"[{h.Chunk.Clause}] {h.Chunk.Text}"));

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, PromptStore.Load("coverage-answer.v1.txt")),
            new(ChatRole.User, $"<clauses>\n{context}\n</clauses>\n\nQuestion: {question}")
        };

        var response = await chat.GetResponseAsync<CoverageAnswer>(
            messages, new ChatOptions { Temperature = 0f }, cancellationToken: ct);

        if (!response.TryGetResult(out var answer)) return null;

        // Verify citations: every cited clause must have been retrieved.
        var retrieved = hits.Select(h => h.Chunk.Clause).ToHashSet();
        if (answer.CitedClauses.Any(c => !retrieved.Contains(c))) return null;

        // An answer claiming enough info must cite at least one clause.
        if (answer.EnoughInformation && answer.CitedClauses.Count == 0) return null;

        return answer;
    }
}
