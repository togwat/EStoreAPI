# Compose
Development:
`docker compose -f docker-compose.yml -f docker-compose.dev.yml up --build`

Production:
`docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d`

# EF-Core Database init scripts
Scripts in `db/init/` are mounted into the `db` container's `/docker-entrypoint-initdb.d/` and run automatically by the postgres image but only on **first initialization** (when the `postgres_data` volume is empty). They currently create the read-only `AGENT_DB_USER` role used by the agent's SQL query tool.

If your database volume already exists, the scripts will not re-run. Apply the script manually once to the running `db` container:

`docker compose exec db /docker-entrypoint-initdb.d/01-agent-readonly.sh`