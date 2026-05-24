from providers.AbstractProvider import ChatProvider
from ollama import Client
from typing import Iterator


class OllamaProvider(ChatProvider):
    def __init__(self, model, host):
        self.model = model
        self.host = host

        self._client = Client(host=host)

        print(f"Provider Ollama connected at {host}. Model: {self.model}")
    
    def chat(self, messages: list[dict]) -> str:
        response = self._client.chat(
            model=self.model, 
            messages=messages
        )
        return response.message.content or "No response returned."

    def stream_chat(self, messages: list[dict]) -> Iterator[str]: 
        for chunk in self._client.chat(
            model=self.model,
            messages=messages,
            stream=True
        ):
            if chunk.message.content:
                yield chunk.message.content

    def chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> tuple[str | None, list[dict], dict | None]:
        """
        Call the model with tools available. Converts definitions.json format
        (input_schema) to Ollama's OpenAI-compatible tool format (parameters).
        """
        ollama_tools = [
            {
                "type": "function",
                "function": {
                    "name": t["name"],
                    "description": t["description"],
                    "parameters": t["input_schema"],
                },
            }
            for t in tools
        ]

        response = self._client.chat(
            model=self.model,
            messages=messages,
            tools=ollama_tools,
        )

        msg = response.message

        # Model chose to call one or more tools
        if msg.tool_calls:
            normalized = [
                {
                    "id": tc.function.name,  # Ollama does not always supply an ID; use name as fallback
                    "name": tc.function.name,
                    "arguments": tc.function.arguments or {},
                }
                for tc in msg.tool_calls
            ]
            # Return the raw message so the loop can append it before tool results
            assistant_msg = {"role": "assistant", "content": msg.content or "", "tool_calls": msg.tool_calls}
            return None, normalized, assistant_msg

        # Model responded with plain text
        return msg.content or "No response returned.", [], None

    def make_tool_result_message(self, tool_name: str, result: str) -> dict:
        """Format a tool result for Ollama's message history."""
        return {"role": "tool", "content": result, "name": tool_name}