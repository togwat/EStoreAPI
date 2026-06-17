from abc import ABC, abstractmethod
from typing import Iterator


class ChatProvider(ABC):
    """Abstract interface for LLM providers. Implement one subclass per provider (Ollama, Anthropic, etc.)."""

    @abstractmethod
    def stream_chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> Iterator[tuple]:
        """
        Stream a model response with tools available. Yields tuples:
          - ("chunk", text: str)                              — a text chunk (stream directly to client)
          - ("tool_calls", tool_calls: list[dict], assistant_msg: dict) — model wants to call tools
        Exactly one of these event types will be produced per call.
        """
        pass

    @abstractmethod
    def make_tool_result_message(self, tool_call_id: str, result: str) -> dict:
        """Format a tool execution result as a message dict for this provider."""
        pass
