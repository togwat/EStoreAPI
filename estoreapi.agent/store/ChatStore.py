"""
Postgres-backed persistence for chat sessions and their full message history.

Each session stores the assistant-ui `ThreadMessage` objects verbatim as JSONB, so a
restored session is a lossless carbon copy (text, reasoning, tool calls + results,
attachments, sources, branches). Sessions are scoped by user email; every read/write is
ownership-checked so users never see each other's chats.
"""
from contextlib import contextmanager

from psycopg2.extras import Json, RealDictCursor
from psycopg2.pool import ThreadedConnectionPool
from store.AbstractChatStore import AbstractChatStore


class ChatStore(AbstractChatStore):
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
        """Create the chat tables and indexes if they don't already exist."""
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                CREATE TABLE IF NOT EXISTS chat_sessions (
                    id              UUID PRIMARY KEY,
                    user_email      TEXT NOT NULL,
                    title           TEXT,
                    head_message_id TEXT,
                    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
                    updated_at      TIMESTAMPTZ NOT NULL DEFAULT now()
                );
                CREATE INDEX IF NOT EXISTS ix_chat_sessions_user_updated
                    ON chat_sessions (user_email, updated_at DESC);

                CREATE TABLE IF NOT EXISTS chat_messages (
                    id                BIGSERIAL PRIMARY KEY,
                    session_id        UUID NOT NULL REFERENCES chat_sessions(id) ON DELETE CASCADE,
                    message_id        TEXT NOT NULL,
                    parent_message_id TEXT,
                    seq               INT NOT NULL,
                    content           JSONB NOT NULL,
                    created_at        TIMESTAMPTZ NOT NULL DEFAULT now()
                );
                CREATE INDEX IF NOT EXISTS ix_chat_messages_session_seq
                    ON chat_messages (session_id, seq);
                """
            )

    # Sessions
    def list_sessions(self, user_email: str) -> list[dict]:
        """All of a user's sessions, most recently updated first."""
        with self._cursor() as cur:
            cur.execute(
                """
                SELECT id, title
                FROM chat_sessions
                WHERE user_email = %s
                ORDER BY updated_at DESC
                """,
                (user_email,),
            )
            return [
                {"remoteId": str(r["id"]), "title": r["title"]}
                for r in cur.fetchall()
            ]

    def create_session(self, user_email: str, thread_id: str) -> str:
        """
        Create a session for the client-provided thread id, or return the existing one.
        Idempotent on the id (assistant-ui may call initialize more than once).
        """
        with self._cursor(commit=True) as cur:
            cur.execute(
                """
                INSERT INTO chat_sessions (id, user_email)
                VALUES (%s, %s)
                ON CONFLICT (id) DO NOTHING
                """,
                (thread_id, user_email),
            )
        return thread_id

    def get_session(self, user_email: str, session_id: str) -> dict | None:
        """Session metadata, or None if it doesn't exist or isn't owned by this user."""
        with self._cursor() as cur:
            cur.execute(
                """
                SELECT id, title, head_message_id
                FROM chat_sessions
                WHERE id = %s AND user_email = %s
                """,
                (session_id, user_email),
            )
            r = cur.fetchone()
            if not r:
                return None
            return {
                "remoteId": str(r["id"]),
                "title": r["title"],
                "headId": r["head_message_id"],
            }

    def rename_session(self, user_email: str, session_id: str, title: str) -> bool:
        with self._cursor(commit=True) as cur:
            cur.execute(
                "UPDATE chat_sessions SET title = %s, updated_at = now() WHERE id = %s AND user_email = %s",
                (title, session_id, user_email),
            )
            return cur.rowcount > 0

    def delete_session(self, user_email: str, session_id: str) -> bool:
        with self._cursor(commit=True) as cur:
            cur.execute(
                "DELETE FROM chat_sessions WHERE id = %s AND user_email = %s",
                (session_id, user_email),
            )
            return cur.rowcount > 0

    # Messages
    def get_messages(self, user_email: str, session_id: str) -> dict | None:
        """
        The session's full message repository in assistant-ui's ExportedMessageRepository
        shape, or None if the session isn't owned by this user.
        """
        session = self.get_session(user_email, session_id)
        if session is None:
            return None
        with self._cursor() as cur:
            cur.execute(
                """
                SELECT message_id, parent_message_id, content
                FROM chat_messages
                WHERE session_id = %s
                ORDER BY seq
                """,
                (session_id,),
            )
            messages = [
                {"message": r["content"], "parentId": r["parent_message_id"]}
                for r in cur.fetchall()
            ]
        return {"headId": session["headId"], "messages": messages}

    def append_message(self, user_email: str, session_id: str, item: dict) -> bool:
        """
        Append one ExportedMessageRepositoryItem ({message, parentId}) to the session.
        Advances the branch head and bumps updated_at in one transaction. The title is
        managed separately by the frontend (generateTitle → rename). False if not owned.
        """
        message = item["message"]
        parent_id = item.get("parentId")
        message_id = message["id"]

        with self._cursor(commit=True) as cur:
            # Ownership + lock the row so concurrent appends get a consistent seq.
            cur.execute(
                "SELECT 1 FROM chat_sessions WHERE id = %s AND user_email = %s FOR UPDATE",
                (session_id, user_email),
            )
            if cur.fetchone() is None:
                return False

            cur.execute(
                "SELECT COALESCE(MAX(seq), -1) + 1 AS next_seq FROM chat_messages WHERE session_id = %s",
                (session_id,),
            )
            next_seq = cur.fetchone()["next_seq"]

            cur.execute(
                """
                INSERT INTO chat_messages (session_id, message_id, parent_message_id, seq, content)
                VALUES (%s, %s, %s, %s, %s)
                """,
                (session_id, message_id, parent_id, next_seq, Json(message)),
            )

            cur.execute(
                "UPDATE chat_sessions SET head_message_id = %s, updated_at = now() WHERE id = %s",
                (message_id, session_id),
            )
        return True