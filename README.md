**Claims AI Assistant with RAG**

**Built a production-style Retrieval-Augmented Generation (RAG) application using ASP.NET Core, Microsoft.Extensions.AI, Ollama, Vector Search, Embeddings, Tool Calling, and Health Insurance Policy Retrieval.**

A Retrieval-Augmented Generation (RAG) based Health Insurance Claims Assistant built using ASP.NET Core Web API, Microsoft.Extensions.AI, Ollama, and Vector Search.

The application helps insurance staff answer policy-related questions by retrieving relevant policy clauses from a vector store and generating grounded responses using an LLM. The solution includes coverage verification, policy lookup, waiting period validation, claims lookup, and clause citation support.

Unlike a traditional chatbot, the assistant retrieves relevant policy information before generating responses, reducing hallucinations and improving answer reliability.

**Key Features**
Policy coverage verification
Claims lookup support
Waiting period validation
Vector-based policy search
Retrieval-Augmented Generation (RAG)
Clause citation in responses
Ollama local LLM integration
Microsoft.Extensions.AI implementation
ASP.NET Core Web API
Hallucination mitigation through retrieval validation


**Prerequisites**

.NET 10 SDK
Ollama
llama3.2 model
nomic-embed-text model
Visual Studio 2026 (or Visual Studio Code)
Git


**Architecture:**
                         ┌────────────────────┐
                         │   User Question    │
                         └──────────┬─────────┘
                                    │
                                    ▼
                     ┌──────────────────────────┐
                     │ Generate Embedding       │
                     │ Microsoft.Extensions.AI  │
                     └──────────┬───────────────┘
                                │
                                ▼
                  ┌──────────────────────────────┐
                  │ In-Memory Vector Store       │
                  │ Similarity Search (Top-K)    │
                  └──────────┬───────────────────┘
                             │
                             ▼
                ┌───────────────────────────────┐
                │ Retrieve Relevant Clauses     │
                │ Policy Chunk Retrieval        │
                └──────────┬────────────────────┘
                           │
                           ▼
              ┌──────────────────────────────────┐
              │ Context Grounding                │
              │ Retrieved Clauses Added Prompt   │
              └──────────┬───────────────────────┘
                         │
                         ▼
               ┌────────────────────────────┐
               │ Ollama LLM                 │
               │ Response Generation        │
               └──────────┬─────────────────┘
                          │
                          ▼
             ┌──────────────────────────────┐
             │ Citation Validation          │
             │ Hallucination Prevention     │
             └──────────┬───────────────────┘
                        │
                        ▼
              ┌────────────────────────────┐
              │ Final Answer               │
              │ With Clause References     │
              └────────────────────────────┘




              
