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
- Models can be locally hosted, all data is kept within the server
- Web search capable

## Running the development version
In `/EStoreAPI.Server`, run:
`dotnet run`

Frontend: https://localhost:5173/
Swagger: http://localhost:5100/swagger/

## Setting up the database
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

Note: these steps are only conducted if Program.cs does not do the migration for you. (so no need for Docker deployment)

