namespace ClaimsAi.Api.Models;



public record BillLineItem(string Description, decimal Amount);

public record ClaimDocument(
    string? PatientName,
    string? Diagnosis,
    DateOnly? AdmissionDate,
    DateOnly? DischargeDate,
    decimal? TotalAmount,
    List<BillLineItem> LineItems);

public record CoverageAnswer(string Answer, List<string> CitedClauses, bool EnoughInformation);

public record Chunk(string Id, string Source, string Clause, string Plan, string Text);

public record PolicyInfo(string PolicyId, string MemberName, string Plan, bool Active,
    decimal SumInsured, decimal Used, DateOnly StartDate);

public record ClaimInfo(string ClaimId, string PolicyId, string Status, decimal Amount, string Diagnosis);

public record ClauseHit(string Clause, string Text, double Score);

public record ChatRequest(string Message);
public record ExtractRequest(string Text);
public record CoverageRequest(string Question, string Plan);
public record RetrievalTest(string Question, string Plan, string ExpectedClause);
