using ClaimsAi.Api.Models;
using Microsoft.Extensions.AI;

namespace ClaimsAi.Api.Services
{
    public class DocumentExtractor(IChatClient chat)
    {
        public async Task<ClaimDocument?> ExtractAsync(string text, CancellationToken ct)
        {
            var messages = new List<ChatMessage>
        {
            new(ChatRole.System, PromptStore.Load("extract-bill.v1.txt")),
            new(ChatRole.User, $"<document>\n{text}\n</document>")
        };

            var response = await chat.GetResponseAsync<ClaimDocument>(
                messages, new ChatOptions { Temperature = 0f }, cancellationToken: ct);

            return response.TryGetResult(out var doc) ? doc : null;
        }

        public static List<string> Validate(ClaimDocument d)
        {
            var p = new List<string>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            if (string.IsNullOrWhiteSpace(d.PatientName)) p.Add("Patient name is missing.");
            if (d.AdmissionDate is null) p.Add("Admission date is missing.");
            if (d.TotalAmount is null) p.Add("Total amount is missing.");
            if (d.TotalAmount is <= 0) p.Add("Total amount must be greater than 0.");
            if (d.LineItems.Any(i => i.Amount < 0)) p.Add("A line item has a negative amount.");
            if (d.AdmissionDate is { } a0 && a0 > today) p.Add("Admission date is in the future.");
            if (d.AdmissionDate is { } a && d.DischargeDate is { } b && b < a)
                p.Add("Discharge date is before admission date.");
            if (d.TotalAmount is { } total && d.LineItems.Count > 0 &&
                Math.Abs(d.LineItems.Sum(i => i.Amount) - total) > 1m)
                p.Add("Line items do not add up to the total.");
            return p;
        }

    }


}
