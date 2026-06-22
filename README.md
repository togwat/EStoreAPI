# EStoreAPI / E-Store Management Console
E-Store Management Console is a web application that facilitates the data management of a phone repair shop. 
## Key Features
**Device model & problem catalogue:**
- Keep track of all device models serviced at the shop
- A list of problems serviced & their base price for each device model
- Easy addition of a new device model and its associated problems
- Easy editing of a device model and its associated problems

**Repair jobs tracking:**
- Easy input of customers and their repair jobs through a form
- Keep track of all repair jobs received by the shop, including search filters by customer or device
- Clean separation of ongoing jobs and finished jobs
- Easy editing of the job's status & data such as the final collected price
- Repair job statistics such as job received per day & total earnings are displayed with charts

**Integrated AI assistant:**
- Assistant chat with access to MCP tools directly integrated within the server, with data search & manipulation capabilities
- Human-in-the-loop safeguards for any data manipulation tools
- File attachments are supported, especially helpful for bulk data import
- Integrated memory layer with **mem0**: the assistant will remember facts from previous chat sessions
- Models can be locally hosted (all data is kept within the server) or from external APIs (fast & no hardware requirements)
- Web search capable
- Persistent chat history

## Deploying the application
**Requirements:**
- Docker engine
For development:
- Python 3.14 or above
- Node.js (npm)
- PostgreSQL + pgvector extension if using memory
- `ASP.NET` 10.0
Optional:
- Ollama (if using OllamaProvider)

**Docker compose:**
Development:
`docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build`

Production:
`docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d`

**Environment variables:**
Create a .env file in the project root with the following template:
```.env
DB_USER=
DB_PASSWORD=
DB_NAME=

# Available providers: ollama, deepseek
PROVIDER=
# Model vars optional if unused
OLLAMA_HOST=
OLLAMA_MODEL=

DEEPSEEK_KEY=
DEEPSEEK_MODEL=

# web search apis
TAVILY_KEY=

# memory (optional if disabled)
MEMORY_ENABLED=true
MEM0_DB_PASSWORD=
MEM0_DB_USER=mem0
MEM0_LLM_MODEL=
MEM0_EMBEDDING_MODEL=

# chat history persistence
CHAT_DB_USER=
CHAT_DB_PASSWORD=
CHAT_DB_NAME=

# read-only db role for the agent SQL tool
AGENT_DB_USER=agent_readonly
AGENT_DB_PASSWORD=

# ASP.NET authentication
# Obtain from google cloud console
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
```
**Google OAuth**:
In Google Cloud Console → APIs and services → Credentials, create an OAuth 2.0 Client ID.
Set authorised redirect URI to http://localhost/signin-google for development, or the server URI {yourdomain}/signin-google for production.

**Whitelist accounts**:
The fresh app will not have any whitelisted emails.
After first successful launch of the app through Docker, go to the db-1 container:
1. `psql -U {username} -d {dbname}` with username and dbname from .env
2. `INSERT INTO "Users" ("Email") VALUES ('email');`
## Restoring database backups
Run the following command:
`gunzip -c backups/{file}.sql.gz | docker exec -i estoreapi-db-1 psql -U {username} {dbname}`
The default backup folder location is usually in the project root.

## Running the development version (no agent, dockerless)
This is for frontend work, so hot reload is available without having to recompose docker images for every change.

In `/EStoreAPI.Server`, run:
`dotnet run`

Frontend: https://localhost:5173/
Swagger: http://localhost:5100/swagger/

## Setting up the database (no agent, dockerless)
Create a test user using postgres admin account:
`createuser -U postgres -P test`

Create a test database using postgres admin account:
`createdb -U postgres estore`

Transfer ownership of database to test:
Enter psql: `psql -U postgres`
```sql
ALTER DATABASE estore OWNER TO test;

\c estore;

GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO test;

ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT ALL ON TABLES TO test;
```

Install EF Core:
`dotnet add package Microsoft.EntityFrameworkCore.Tools`

Install tool globally:
`dotnet tool install --global dotnet-ef`

Install Npgsql provider:
`dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL`

Update database:
`dotnet ef database update`

