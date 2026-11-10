using System.ComponentModel;
using ClaimsAi.Api.Models;
using Microsoft.Extensions.AI;

namespace ClaimsAi.Api.Services;

public interface IPolicyService { Task<PolicyInfo?> GetAsync(string policyId); }
public interface IClaimService { Task<ClaimInfo?> GetAsync(string claimId); }

public class FakePolicyService : IPolicyService
{
    private static readonly Dictionary<string, PolicyInfo> Data = new(StringComparer.OrdinalIgnoreCase)
    {
        ["P100"] = new("P100", "Asha Raman", "Gold-2", true, 500000m, 120000m, new DateOnly(2024, 4, 1)),
        ["P200"] = new("P200", "Ravi Kumar", "Silver-1", false, 300000m, 0m, new DateOnly(2023, 1, 15)),
    };
    public Task<PolicyInfo?> GetAsync(string id) => Task.FromResult(Data.GetValueOrDefault(id));
}

public class FakeClaimService : IClaimService
{
    private static readonly Dictionary<string, ClaimInfo> Data = new(StringComparer.OrdinalIgnoreCase)
    {
        ["C5"] = new("C5", "P100", "UnderReview", 85000m, "Appendicitis"),
    };
    public Task<ClaimInfo?> GetAsync(string id) => Task.FromResult(Data.GetValueOrDefault(id));

    // this is realtime DB calling

    //public async Task<ClaimInfo?> GetAsync(string claimId)
    //{
    //    var client = _httpClientFactory.CreateClient();

    //    return await client.GetFromJsonAsync<ClaimInfo>(
    //        $"https://claims-api.company.com/claims/{claimId}");
    //}
}

public class ClaimTools(
    IPolicyService policies,
    IClaimService claims,
    IEmbeddingGenerator<string, Embedding<float>> embedder,
    InMemoryVectorStore store
    )
{
    [Description("Gets a policy: active status, plan name, sum insured, amount already used and start date.")]
    public async Task<PolicyInfo?> GetPolicy([Description("The policy ID, for example P100")] string policyId)
        => await policies.GetAsync(policyId);

    [Description("Gets the status, amount and diagnosis of a claim.")]
    public async Task<ClaimInfo?> GetClaim([Description("The claim ID, for example C5")] string claimId)
        => await claims.GetAsync(claimId);

    [Description("Calculates remaining sum insured for a policy. Returns null if the policy does not exist.")]
    public async Task<decimal?> GetRemainingBalance([Description("The policy ID")] string policyId)
    {
        var p = await policies.GetAsync(policyId);
        return p is null ? null : p.SumInsured - p.Used;
    }

    //Q1 :Policy ID P100. Admission date 2026-05-20. This is a pre-existing condition. Has the waiting period been completed?
    //Q2 :Patient is covered under policy P100. He was admitted on 2024-06-15 for dengue fever. The disease is not pre-existing. Please check whether the waiting period has been satisfied. 
    [Description("Checks the initial waiting period rule for a policy: 30 days from the policy start for general conditions, 24 months for pre-existing conditions.")]
    public async Task<string> CheckWaitingPeriod(
        [Description("The policy ID")] string policyId,
        [Description("The admission date, format yyyy-MM-dd")] string admissionDate,
        [Description("True if the diagnosis is a pre-existing condition")] bool preExisting)
    {
        var p = await policies.GetAsync(policyId);
        if (p is null) return "NOT_FOUND";
        if (!DateOnly.TryParse(admissionDate, out var adm)) return "INVALID_DATE";
        var eligibleFrom = preExisting ? p.StartDate.AddMonths(24) : p.StartDate.AddDays(30);
        return adm >= eligibleFrom
            ? $"Waiting period satisfied (eligible from {eligibleFrom:yyyy-MM-dd})."
            : $"Waiting period NOT satisfied (eligible from {eligibleFrom:yyyy-MM-dd}).";
    }

    [Description("Searches the policy wording of a plan for clauses relevant to a query. Returns clause numbers and text.")]
    public async Task<List<ClauseHit>> SearchPolicyClauses(
        [Description("What to look for, for example 'knee replacement waiting period'")] string query,
        [Description("Plan name, for example Gold-2")] string plan)
    {
        var q = await embedder.GenerateAsync([query]);
        return store.Search(q[0].Vector.ToArray(), topK: 4, plan: plan)
            .Select(h => new ClauseHit(h.Chunk.Clause, h.Chunk.Text, Math.Round(h.Score, 3)))
            .ToList();
    }

    public IList<AITool> AsTools() =>
    [
        AIFunctionFactory.Create(GetPolicy),
        AIFunctionFactory.Create(GetClaim),
        AIFunctionFactory.Create(GetRemainingBalance),
        AIFunctionFactory.Create(CheckWaitingPeriod),
        AIFunctionFactory.Create(SearchPolicyClauses)
    ];
}

