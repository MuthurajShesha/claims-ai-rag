using System.Text.RegularExpressions;
using ClaimsAi.Api.Models;

namespace ClaimsAi.Api.Services;

public static class PolicyChunker
{
    // Splits on "Clause X.Y" and keeps the clause number as metadata.
    public static List<Chunk> Split(string source, string plan, string text)
    {
        var chunks = new List<Chunk>();
        foreach (var part in Regex.Split(text, @"(?=Clause \d+\.\d+)"))
        {
            var trimmed = part.Trim();
            var m = Regex.Match(trimmed, @"^Clause (\d+\.\d+)");
            if (!m.Success) continue;
            chunks.Add(new Chunk($"{source}:{m.Groups[1].Value}", source, m.Groups[1].Value, plan, trimmed));
        }
        return chunks;
    }
}
