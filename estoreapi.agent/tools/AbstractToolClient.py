from abc import ABC, abstractmethod


class AbstractToolClient(ABC):
    @abstractmethod
    def list_tools(self) -> list[dict]:
        """Return tools in {name, description, input_schema} format expected by providers."""
        pass

    @abstractmethod
    def call_tool(self, name: str, arguments: dict) -> str:
        """Call a named tool and return the result as a string."""
        pass