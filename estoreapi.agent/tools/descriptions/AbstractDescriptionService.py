from abc import ABC, abstractmethod


class AbstractDescriptionService(ABC):
    @abstractmethod
    def get(self, tool_name: str) -> str | None:
        """Return the override description for a tool, or None if not set."""
        pass

    @abstractmethod
    def update(self, tool_name: str, description: str) -> str:
        """Persist a new description override and return a confirmation message."""
        pass
