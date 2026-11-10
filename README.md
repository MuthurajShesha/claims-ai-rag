# claims-ai-rag
Claims AI Assistant with RAG
Health insurance claims assistant built using:

ASP.NET Core Web API (.NET 10)
Microsoft.Extensions.AI
Ollama
Vector Search
Retrieval Augmented Generation (RAG)
Tool Calling
Features
Policy lookup
Claim lookup
Waiting period validation
Coverage checks
Clause citation
Vector-based document retrieval
Architecture
User Question ↓ Embedding Generation ↓ Vector Search ↓ Top-K Retrieval ↓ Context Injection ↓ LLM Response ↓ Citation Validation

Sample Question
Does Platinum-3 cover maternity?

Sample Response
According to Clause 5.1, maternity expenses are covered after a waiting period of 24 months, up to 100000 per delivery.

Tech Stack
ASP.NET Core
Microsoft.Extensions.AI
OllamaSharp
InMemoryVectorStore
