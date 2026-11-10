using ClaimsAi.Api.Models;
using Microsoft.Extensions.AI;

namespace ClaimsAi.Api.Services;

public class PolicyIndexer(
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    InMemoryVectorStore store,
    ILogger<PolicyIndexer> log)
{
    public async Task IndexAsync(string source, string plan, string text, CancellationToken ct = default)
    {
        var chunks = PolicyChunker.Split(source, plan, text);
        if (chunks.Count == 0) return;

        var embeddings = await embedder.GenerateAsync(chunks.Select(c => c.Text).ToList(), cancellationToken: ct);
        for (var i = 0; i < chunks.Count; i++)
            store.Add(chunks[i], embeddings[i].Vector.ToArray());

        log.LogInformation("Indexed {Count} clauses from {Source} (plan {Plan})", chunks.Count, source, plan);
    }

    // Plan name comes from the file name: gold-2.txt -> "Gold-2"
    public async Task IndexAllAsync(CancellationToken ct = default)
    {
        var dir = Path.Combine(Path.Combine(Directory.GetCurrentDirectory()), "Data");
        foreach (var file in Directory.GetFiles(dir, "*.txt"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (name.StartsWith("sample")) continue; // sample bills are not policies
            var plan = string.Join("-", name.Split('-').Select(p => char.ToUpper(p[0]) + p[1..]));
            await IndexAsync(name, plan, await File.ReadAllTextAsync(file, ct), ct);
        }
    }
}

// Indexes policies at startup; retries until Ollama is reachable.
public class IndexingHostedService(IServiceProvider sp, ILogger<IndexingHostedService> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        for (var attempt = 1; attempt <= 10 && !ct.IsCancellationRequested; attempt++)
        {
            try
            {
                await sp.GetRequiredService<PolicyIndexer>().IndexAllAsync(ct);
                log.LogInformation("Policy indexing finished.");
                return;
            }
            catch (Exception ex)
            {
                log.LogWarning("Indexing attempt {Attempt} failed: {Msg}. Is Ollama running and nomic-embed-text pulled?",
                    attempt, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }
}
