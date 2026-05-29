from abc import ABC, abstractmethod


class AbstractDescriptionService(ABC):
    @abstractmethod
    def get(self, tool_name: str, param_name: str | None = None) -> str | None:
        """Return the description for a tool, or None if not set."""
        pass

    @abstractmethod
    def update(self, tool_name: str, description: str, param_name: str | None = None) -> str:
        """Replace the  return a confirmation message."""
        pass
