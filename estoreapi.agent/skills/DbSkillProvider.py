from contextlib import contextmanager

from psycopg2.extras import RealDictCursor
from psycopg2.pool import ThreadedConnectionPool

from skills.SkillProvider import SkillProvider


class DbSkillProvider(SkillProvider):
    """
    Postgres skill storage.
    """

    def __init__(self, host: str, port: int, user: str, password: str, dbname: str):
        # Threaded pool: FastAPI may serve sync endpoints from a thread pool.
        self._pool = ThreadedConnectionPool(
            minconn=1,
            maxconn=10,
            host=host,
            port=port,
            user=user,
            password=password,
            dbname=dbname,
        )

    @contextmanager
    def _cursor(self, commit: bool = False):
        """Borrow a pooled connection and yield a dict cursor, returning it afterwards."""
        conn = self._pool.getconn()
        try:
            with conn.cursor(cursor_factory=RealDictCursor) as cur:
                yield cur
            if commit:
                conn.commit()
        except Exception:
            conn.rollback()
            raise
        finally:
            self._pool.putconn(conn)

    def _unknown_skill(self, name: str) -> str:
        """Miss message that lists what does exist, so the model can self-correct."""
        names = [skill["name"] for skill in self.list_skills()]

        if not names:
            return f"Unknown skill '{name}'. No skills are saved yet."
        
        return f"Unknown skill '{name}'. Available skills: {', '.join(names)}"

    def init_schema(self) -> None:
        """
        Create the skills table if it doesn't already exist.

        use_count, last_used_at, created_at, updated_at are currently unused metadata.
        """
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                CREATE TABLE IF NOT EXISTS skills (
                    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    name         TEXT NOT NULL UNIQUE,
                    description  TEXT NOT NULL,
                    content      TEXT NOT NULL,
                    use_count    INT NOT NULL DEFAULT 0,
                    last_used_at TIMESTAMPTZ,
                    created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
                    updated_at   TIMESTAMPTZ NOT NULL DEFAULT now()
                );
                """
            )

    def list_skills(self) -> list[dict]:
        """
        Returns a list of all skills in the format:
        {name, description}
        """
        with self._cursor() as cur:
            cur.execute("SELECT name, description FROM skills ORDER BY name")
            return [dict(row) for row in cur.fetchall()]

    def get_skill(self, name: str) -> str:
        """Retrieve the skill document with the given name"""
        # Update use_count and last_used at in addition to retrieval
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                UPDATE skills
                SET use_count = use_count + 1, last_used_at = now()
                WHERE name = %s
                RETURNING content
                """,
                (name,),
            )
            row = cur.fetchone()

        if row is None:
            return self._unknown_skill(name)
        
        return f"# {name}\n\n{row['content']}"

    def create_skill(self, name: str, summary: str, content: str) -> str:
        """
        Create a skill document.

        name: the unique id of the skill, used for retrieval
        summary: short summary of the skill that is always fed to the agent, so it knows when to get this skill.
        content: hidden to the agent until retrieved

        Returns a confirmation message.
        """
        # ON CONFLICT + rowcount instead of checking existence first: race-safe under concurrent requests
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                INSERT INTO skills (name, description, content)
                VALUES (%s, %s, %s)
                ON CONFLICT (name) DO NOTHING
                """,
                (name, summary, content),
            )
            created = cur.rowcount == 1

        if not created:
            return f"Skill '{name}' already exists. Use update_skill to modify it, or pick a different name."
        
        return f"Skill '{name}' created."

    def update_skill(self, name: str, summary: str | None = None, content: str | None = None) -> str:
        """
        Update a skill's summary or content. If either are empty/none, the fields stay as-is.

        Returns a confirmation message.
        """
        # Normalise empty strings to None
        summary = summary or None
        content = content or None

        if summary is None and content is None:
            return "Nothing to update: provide summary and/or content."

        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                UPDATE skills
                SET description = COALESCE(%s, description),
                    content     = COALESCE(%s, content),
                    updated_at  = now()
                WHERE name = %s
                """,
                (summary, content, name),
            )
            updated = cur.rowcount == 1

        if not updated:
            return self._unknown_skill(name)
        
        return f"Skill '{name}' updated."

    def delete_skill(self, name: str) -> str:
        """
        Deletes the skill with the given name.
        Returns a confirmation message.
        """
        with self._cursor(commit=True) as cur:
            cur.execute("DELETE FROM skills WHERE name = %s", (name,))
            deleted = cur.rowcount == 1

        if not deleted:
            return self._unknown_skill(name)
        
        return f"Skill '{name}' deleted."