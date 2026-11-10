using ClaimsAi.Api.Services;
using Microsoft.Extensions.AI;
using OllamaSharp;

var builder = WebApplication.CreateBuilder(args);

var endpoint = new Uri(builder.Configuration["Ollama:Endpoint"] ?? "http://localhost:11434");
var chatModel = builder.Configuration["Ollama:ChatModel"] ?? "llama3.2";
var embedModel = builder.Configuration["Ollama:EmbeddingModel"] ?? "nomic-embed-text";

// Add services to the container.
builder.Services.AddControllers();

builder.Services
    .AddChatClient(sp => new OllamaApiClient(endpoint, chatModel))
    .UseFunctionInvocation()
    .UseLogging();

builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(
    _ => new OllamaApiClient(endpoint, embedModel));

builder.Services.AddSingleton<InMemoryVectorStore>();
builder.Services.AddSingleton<PolicyIndexer>();
builder.Services.AddHostedService<IndexingHostedService>();

builder.Services.AddSingleton<IPolicyService, FakePolicyService>();
builder.Services.AddSingleton<IClaimService, FakeClaimService>();
builder.Services.AddSingleton<ClaimTools>();
builder.Services.AddSingleton<DocumentExtractor>();
builder.Services.AddSingleton<CoverageService>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
