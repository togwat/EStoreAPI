from abc import ABC, abstractmethod


class AbstractChatStore(ABC):
    """
    Persistence for chat sessions and their full message history.

    A session holds the assistant-ui message repository verbatim so a restored chat is a
    lossless carbon copy (text, reasoning, tool calls + results, attachments, sources,
    branches). All operations are scoped by user email and ownership-checked, so users
    never see each other's chats. Swap the backing store by implementing this interface.
    """

    @abstractmethod
    def init_schema(self) -> None:
        """Prepare the backing storage (e.g. create tables)."""
        pass

    # Sessions
    @abstractmethod
    def list_sessions(self, user_email: str) -> list[dict]:
        """A user's sessions as `{remoteId, title}`, most recently updated first."""
        pass

    @abstractmethod
    def create_session(self, user_email: str) -> str:
        """Create a new session and return the remoteId."""
        pass

    @abstractmethod
    def get_session(self, user_email: str, session_id: str) -> dict | None:
        """`{remoteId, title, headId}`, or None if it doesn't exist / isn't owned."""
        pass

    @abstractmethod
    def rename_session(self, user_email: str, session_id: str, title: str) -> bool:
        """Set a session's title. Returns False if not owned."""
        pass

    @abstractmethod
    def delete_session(self, user_email: str, session_id: str) -> bool:
        """Delete a session and its messages. Returns False if not owned."""
        pass

    # Messages
    @abstractmethod
    def get_messages(self, user_email: str, session_id: str) -> dict | None:
        """
        The session's messages as an assistant-ui ExportedMessageRepository
        (`{headId, messages: [{message, parentId}]}`), or None if not owned.
        """
        pass

    @abstractmethod
    def append_message(self, user_email: str, session_id: str, item: dict) -> bool:
        """
        Append one ExportedMessageRepositoryItem (`{message, parentId}`), advancing the
        branch head and deriving the title from the first user message. False if not owned.
        """
        pass