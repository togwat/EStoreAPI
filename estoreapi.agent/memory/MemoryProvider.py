from abc import ABC, abstractmethod


class MemoryProvider(ABC):
    @abstractmethod
    def get_context(self, user_id: str) -> str:
        """Returns memory text to be injected into the system prompt at session start."""
        pass

    def write(self, messages: list[dict], user_id: str) -> None:
        """
        Persists the completed conversation turn to memory.
        Should be called at the end of every chat turn.
        """
        pass

    def search(self, query: str, user_id: str) -> str:
        """
        Searches memory for matches against the query.
        Exposed as a callable tool rather than a background function.
        """
        return ""
