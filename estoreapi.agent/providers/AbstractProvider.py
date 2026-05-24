from abc import ABC, abstractmethod
from typing import Iterator


class ChatProvider(ABC):
    """Abstract interface for LLM providers. Implement one subclass per provider (Ollama, Anthropic, etc.)."""

    @abstractmethod
    def chat(self, messages: list[dict]) -> str:
        pass

    @abstractmethod
    def stream_chat(self, messages: list[dict]) -> Iterator[str]:
        pass

    @abstractmethod
    def chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> tuple[str | None, list[dict], dict | None]:
        """
        Send messages to the model with tools available.

        Returns a 3-tuple:
          - text_response  (str)       — final text reply, or None if tool calls were made
          - tool_calls     (list[dict]) — list of {id, name, arguments}, empty if text reply
          - assistant_msg  (dict)       — raw assistant message to append before tool results,
                                         or None when text_response is set
        """
        pass

    @abstractmethod
    def make_tool_result_message(self, tool_name: str, result: str) -> dict:
        """Format a tool execution result as a message dict for this provider."""
        pass


class EmbeddingProvider(ABC):
    @abstractmethod
    def embed(self):
        pass
