# TCS AI Chat — Backend Service

A multi-tenant AI chat backend built with **Microsoft Semantic Kernel** and **.NET 8**. One service powers the AI assistant for multiple TCS frontends, each with its own agent persona, capabilities, and context.

![Demo](gifs/EstimateCalculation.webp)

---

## What It Does

Each frontend sends a `clientId` when opening a chat session. The backend selects the right agent persona and available tools for that client, then routes every message through an OpenAI model with function-calling enabled. The LLM can invoke native C# plugins to calculate estimates, edit images, or diagnose appliances — all in the same response loop.

**Supported clients:**

| Client | `clientId` | AI Capabilities |
| --- | --- | --- |
| [TCS Paints](https://github.com/NikitaaOvramenko/TCS---Paints) | `tcs-paints` | Paint cost estimation, AI image recoloring |
| [TCS Junk Removal](https://github.com/NikitaaOvramenko/TCS-Junk-Removal) | `tcs-junk-removal` | Removal cost estimation, image-based item recognition |
| [TCS Appliance Repair](https://github.com/NikitaaOvramenko/applience-repair-site) | `appliance-repair` | Appliance diagnostic, service routing |

---

## Tech Stack

- **.NET 8** — Web API
- **Microsoft Semantic Kernel** v1.63.0 — AI orchestration, plugin system, function calling
- **OpenAI** — GPT chat completion (structured JSON output mode)
- **Google Gemini** — Image generation for surface recoloring (`gemini-2.0-flash-preview-image-generation`)
- **Supabase** — File storage for uploaded and edited images
- **Python** (subprocess) — Bridge between C# and the Gemini image generation API

---

## Architecture

```text
Frontend  ──POST /api/session/GetSession──►  SessionController
                (clientId + sessionId)              │
                                                    ▼
                                            TenantRegistry
                                        (system prompt lookup)
                                                    │
                                                    ▼
Frontend  ──POST /api/session/WriteToChat──►  ChatManager
                (message + optional image)     (in-memory sessions)
                                                    │
                                                    ▼
                                            SemanticKernel
                                         (OpenAI + all plugins)
                                                    │
                                          ┌─────────┴──────────┐
                                          ▼                     ▼
                                    C# Plugins           ImageSurfaceColor
                                  (Estimate,              (Python → Gemini
                               JunkRemovalEstimate,        → Supabase)
                               ApplianceDiagnostic)
```

**`TenantRegistry`** maps each `clientId` to a `TenantConfig` containing the agent's system prompt and (optionally) an image-upload instruction. Adding a new client is one dictionary entry.

**`ChatManager`** holds all active sessions in a `ConcurrentDictionary`. Sessions are in-memory — they are lost on server restart (by design for now).

**Plugins** are all registered on a single global kernel. The system prompt for each tenant instructs the LLM which functions to call and when.

---

## API

All endpoints are under `/api/session`. Swagger UI is available at `/swagger` in development.

### `POST /api/session/GetSession`

Creates a new chat session and injects the tenant's system prompt.

```json
{
  "id": "user-session-abc123",
  "clientId": "tcs-paints"
}
```

Returns the session ID string on success, `400` if `clientId` is unknown.

---

### `POST /api/session/WriteToChat`

Sends a message (and optionally an image). Returns a JSON-serialized `ResponseFormat`.

Sent as `multipart/form-data`:

| Field | Type | Description |
| --- | --- | --- |
| `Id` | string | Session ID from `GetSession` |
| `MessageT` | string | User's message text |
| `Image` | file (optional) | Image upload (blocked for `appliance-repair`) |
| `Author` | string | Sender label |

Response:

```json
{
  "message": "Estimated removal cost: $190.00 (2 cu yd × $80 + 1 heavy item × $30)",
  "url": null
}
```

For image edits, `url` contains the Supabase public URL of the modified image.

---

### `POST /api/session/EndChat`

Removes the session from memory.

```json
{ "id": "user-session-abc123", "clientId": "tcs-paints" }
```

Returns `204 No Content`.

---

## Setup

### 1. Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Python 3 with packages: `google-genai`, `Pillow`, `requests`
- A Supabase project with a public `media` storage bucket

### 2. Environment Variables

Create a `.env` file inside `backend-sk-chat-tcs/` (next to the `.csproj`):

```env
OPENAI_APIKEY=sk-...
MODEL_NAME=gpt-4o-mini
GEMINI_APIKEY=AIza...
SUPBASE_URL=https://your-project.supabase.co
SUPBASE_KEY=your-service-role-key
PAGE_URLS=http://localhost:5173,http://localhost:5174,http://localhost:5175
PYTHON_CHOICE=python
```

> **`SUPBASE_URL` / `SUPBASE_KEY`** — the typo is intentional and consistent throughout the codebase. Do not correct it.
> **`PAGE_URLS`** — comma-separated list of frontend origins allowed by CORS.

### 3. Run

```bash
dotnet run --project backend-sk-chat-tcs/backend-sk-chat-tcs.csproj
```

Swagger opens at `http://localhost:5033/swagger`.

---

## Plugins

| Plugin | Function | Used by |
| --- | --- | --- |
| `Estimate` | `CalcWallPrice(width, height, coats)` | `tcs-paints` |
| `ImageSurfaceColor` | `EditImageAsync(instruction, publicUrl)` | `tcs-paints` |
| `JunkRemovalEstimate` | `EstimateJunkRemoval(volumeCubicYards, heavyItemCount)` | `tcs-junk-removal` |
| `ApplianceDiagnostic` | `DiagnoseAppliance(applianceType, symptoms)` | `appliance-repair` |

Image editing pipeline: `ImageSurfaceColor` spawns `Plugins/Native/script.py`, which calls Gemini image generation, receives the result as Base64, uploads it to Supabase, and returns the public URL.

---

## Roadmap

- **Persistence** — Move chat history to Supabase DB so sessions survive restarts
- **Streaming** — Expose the existing `IAsyncEnumerable` stream to the frontend for real-time token output
- **Auth** — Validate frontend requests with a shared secret or JWT
