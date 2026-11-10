using ClaimsAi.Api.Models;

namespace ClaimsAi.Api.Services;

public class InMemoryVectorStore
{
    private readonly List<(Chunk Chunk, float[] Vector)> _items = new();
    private readonly object _lock = new();

    public int Count { get { lock (_lock) return _items.Count; } }

    public void Add(Chunk chunk, float[] vector)
    {
        lock (_lock)
        {
            _items.RemoveAll(i => i.Chunk.Id == chunk.Id); // re-indexing is idempotent
            _items.Add((chunk, vector));
        }
    }

    public List<(Chunk Chunk, double Score)> Search(float[] query, int topK, string? plan = null)
    {
        lock (_lock)
        {
            return _items
                .Where(i => plan is null || string.Equals(i.Chunk.Plan, plan, StringComparison.OrdinalIgnoreCase))
                .Select(i => (i.Chunk, Score: Cosine(query, i.Vector)))
                .OrderByDescending(x => x.Score)
                .Take(topK)
                .ToList();
        }
    }

    private static double Cosine(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }
        return dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-12);
    }
}
