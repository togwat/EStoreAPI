# EStoreAPI

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
