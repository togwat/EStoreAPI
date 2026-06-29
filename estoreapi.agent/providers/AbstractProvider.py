from abc import ABC, abstractmethod
from typing import Iterator


class ChatProvider(ABC):
    """Abstract interface for LLM providers. Implement one subclass per provider (Ollama, Anthropic, etc.)."""

    @property
    @abstractmethod
    def context_window(self) -> int | None:
        """
        Get the maximum context window in tokens of the active model, or None if unknown
        (so the UI can show "unknown" rather than a fabricated limit).
        """
        pass

    @abstractmethod
    def stream_chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> Iterator[tuple]:
        """
        Stream a model response with tools available. Yields tuples:
          - ("chunk", text: str)                              — a text chunk (stream directly to client)
          - ("usage", usage: dict)                            — token counts for the call (optional, provider-dependent)
          - ("tool_calls", tool_calls: list[dict], assistant_msg: dict) — model wants to call tools
        Exactly one of the chunk/tool_calls event types will be produced per call.
        """
        pass

    @abstractmethod
    def make_tool_result_message(self, tool_call_id: str, result: str) -> dict:
        """Format a tool execution result as a message dict for this provider."""
        pass
