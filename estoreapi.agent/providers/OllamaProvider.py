from providers.AbstractProvider import ChatProvider
from ollama import Client
from typing import Iterator


class OllamaProvider(ChatProvider):
    def __init__(self, model, sys_prompt, host):
        self.model = model
        self.sys_prompt = sys_prompt
        self.host = host

        self._client = Client(host=host)

        print(f"Provider Ollama connected at {host}. Model: {self.model}")
    
    def chat(self, messages: list[dict]) -> str:
        response = self._client.chat(model=self.model, messages=messages)
        return response.message.content or "No response returned."

    def stream_chat(self, messages: list[dict]) -> Iterator[str]: 
        for chunk in self._client.chat(model=self.model, messages=messages, stream=True):
            if chunk.message.content:
                yield chunk.message.content