from abc import ABC, abstractmethod
from typing import Iterator


class ChatProvider(ABC):
    @abstractmethod
    def chat(self, messages: list[dict]) -> str:
        pass

    @abstractmethod
    def stream_chat(self, messages: list[dict]) -> Iterator[str]:
        pass


class EmbeddingProvider(ABC):
    @abstractmethod
    def embed(self):
        pass
