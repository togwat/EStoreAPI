# E-Store Management Console — 1.0 Release

---

## What's in 1.0

This release delivers the core repair shop management system, replacing the legacy PHP application.

### Customer Management
- Create, read, update, and delete customer records
- Search customers by name or phone number

### Device Management
- Create, read, and update device models
- Search devices by name or type
- Associate known problems with device models

### Problem Catalogue
- Maintain a per-device list of common problems, each with a set price
- Add, update, and remove problems and prices

### Job Management
- Create a job linking a customer, a device, and one or more problems
- Support jobs where the problem is unknown at intake
- Record receive time; set estimated and actual pickup times
- Capture an estimated price and a final collected price
- Mark a job as completed

### Authentication
- Google OAuth login — all routes protected behind an authenticated session
- User accounts created and managed administratively (no self-registration)

### Frontend
- Login page with Google OAuth flow
- Job intake form: links customer, device, and selected problems in one submission; estimated price pre-filled from selected problems, overridable by the technician
- Job list view showing outstanding and completed jobs
- Customer lookup for repair history

### AI Agent
- Persistent chatbox accessible from anywhere in the app
- File upload support (images, documents, spreadsheets) for single or bulk data import
- Tool-calling over the full CRUD API surface — the agent can create and modify data from natural language prompts, with staff confirmation required before writes
- Readonly query capability in the database for more specific data lookup
- Has web search capabilities for up-to-date information from the real world

---

## Out of Scope for 1.0

The following items were identified during planning but are deferred to a future release:

| Feature | Notes |
|---------|-------|
| One-off device support | Jobs for obscure devices not in the catalogue require a device record to be created first. An ad-hoc solution used is to make an 'other' device and keep the device model in notes |
| Warranty tracking | Warranty status on completed jobs and warranty-linked job creation not yet implemented |
| AI page navigation | Agent cannot yet redirect the user to a specific page or apply filters automatically |
| External partner tracking | Not in scope for 1.0 |
| Parts/stock management | Not in scope for 1.0 |


---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | `ASP.NET` Core 10 (C#) |
| Database | PostgreSQL via Entity Framework Core 10 |
| Frontend | React + Vite (TypeScript) |
| AI agent | Python (Ollama, locally hosted or private cloud) |
| AI interface | assistant-ui |
| AI memory | mem0 |
| AI Web search | Tavily |
| Authentication | Google OAuth |
| Containerisation | Docker (5 services: API, client, DB, agent, memory DB) |
| API docs | Swagger / OpenAPI |

---

## Known Limitations

- The system is an internal staff tool only; there is no customer-facing interface.
- User account management is administrative — no self-service signup or password reset.
- AI agent actions that modify data require explicit staff confirmation via the chatbox.
- Device support requires a device model entry in the catalogue; one-off repairs must have a record created first.

---

## Future Work

- Warranty status tracking and warranty-linked jobs
- AI-driven page navigation and filter application
- Tracking jobs handed off to external partner shops
- Parts and stock inventory
- Stale customer record cleanup (privacy policy: 5 years of inactivity with no active warranty)
