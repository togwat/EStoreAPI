from contextlib import contextmanager

from psycopg2.extras import RealDictCursor
from psycopg2.pool import ThreadedConnectionPool

from skills.SkillProvider import SkillProvider
from skills.markdown_parser import from_markdown, to_markdown


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
                    summary  TEXT NOT NULL,
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
        {name, summary}
        """
        with self._cursor() as cur:
            cur.execute("SELECT name, summary FROM skills ORDER BY name")
            return [dict(row) for row in cur.fetchall()]

    def get_skill(self, name: str) -> str | None:
        """
        Retrieve the skill document with the given name.

        Returns None if no skill has that name.
        """
        with self._cursor() as cur:
            cur.execute("SELECT name, summary, content FROM skills WHERE name = %s", (name,))
            row = cur.fetchone()

        if row is None:
            return None

        return to_markdown(dict(row))

    def create_skill(self, name: str, file: str) -> bool:
        """
        Create a skill document.

        name: the unique id of the skill, used for retrieval
        summary: short summary of the skill that is always fed to the agent, so it knows when to get this skill.
        Held in frontmatter as 'summary'.
        content: hidden to the agent until retrieved

        Returns False if the name is already taken.
        Raises ValueError if the document is not in the expected format.
        """
        skill = from_markdown(name, file)

        # ON CONFLICT + rowcount instead of checking existence first: race-safe under concurrent requests
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                INSERT INTO skills (name, summary, content)
                VALUES (%s, %s, %s)
                ON CONFLICT (name) DO NOTHING
                """,
                (skill["name"], skill["summary"], skill["content"]),
            )
            created = cur.rowcount == 1

        return created

    def update_skill(self, name: str, file: str) -> bool:
        """
        Update a skill by overwriting its document.
]
        Returns False if no skill has that name.
        Raises ValueError if the document is not in the expected format.
        """
        skill = from_markdown(name, file)

        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                UPDATE skills
                SET summary = %s,
                    content = %s,
                    updated_at = now()
                WHERE name = %s
                """,
                (skill["summary"], skill["content"], name),
            )
            updated = cur.rowcount == 1

        return updated

    def delete_skill(self, name: str) -> bool:
        """
        Deletes the skill with the given name.

        Returns False if no skill has that name.
        """
        with self._cursor(commit=True) as cur:
            cur.execute("DELETE FROM skills WHERE name = %s", (name,))
            deleted = cur.rowcount == 1

        return deleted

    def record_use(self, name: str) -> None:
        """
        Increment the skill's use count by one and set last used time to present time.
        For use when the agent calls get_skill.
        """
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                UPDATE skills
                SET use_count = use_count + 1, last_used_at = now()
                WHERE name = %s
                """,
                (name,),
            )