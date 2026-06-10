#!/bin/bash
# Creates the read-only role used by the agent's SQL query MCP tool.
#
# The official postgres image runs this automatically (via
# /docker-entrypoint-initdb.d) on FIRST initialization only, i.e. when the
# postgres_data volume is empty. For an already-initialized volume, apply it
# manually once:
#   docker compose exec db /docker-entrypoint-initdb.d/01-agent-readonly.sh
set -euo pipefail

# psql -v variables are used instead of shell interpolation inside the SQL so
# special characters in the password cannot break out of the quoting.
psql -v ON_ERROR_STOP=1 \
     -v agent_user="$AGENT_DB_USER" \
     -v agent_password="$AGENT_DB_PASSWORD" \
     -v db="$POSTGRES_DB" \
     --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<'EOSQL'
-- Create the role only if it doesn't exist yet, so manual re-runs are safe.
SELECT format('CREATE ROLE %I LOGIN PASSWORD %L', :'agent_user', :'agent_password')
WHERE NOT EXISTS (SELECT FROM pg_roles WHERE rolname = :'agent_user')
\gexec

GRANT CONNECT ON DATABASE :"db" TO :"agent_user";
GRANT USAGE ON SCHEMA public TO :"agent_user";

-- The API applies a per-table allowlist after EF migrations run (see Data/AgentReadOnlyGrants.cs),
-- tables added by future migrations (e.g. auth) are unreadable by default.

-- Defense in depth on top of the SELECT-only grants: refuse writes even if a
-- future migration broadens permissions, and bound runaway queries.
ALTER ROLE :"agent_user" SET default_transaction_read_only = on;
ALTER ROLE :"agent_user" SET statement_timeout = '5s';
EOSQL
