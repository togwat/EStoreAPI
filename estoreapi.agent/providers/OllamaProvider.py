from providers.AbstractProvider import ChatProvider
from ollama import Client
from typing import Iterator


class OllamaProvider(ChatProvider):
    def __init__(self, model, host):
        self.model = model
        self.host = host
        self._client = Client(host=host)
        print(f"Provider Ollama connected at {host}. Model: {self.model}")

    def _to_ollama_tools(self, tools: list[dict]) -> list[dict]:
        """Convert definitions.json format (input_schema) to Ollama tool format (parameters)."""
        return [
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

    def _normalize_tool_calls(self, tool_calls) -> list[dict]:
        """Normalize Ollama tool_calls to {id, name, arguments} dicts."""
        return [
            {
                "id": tc.function.name,   # Ollama doesn't always supply an ID; name is the fallback
                "name": tc.function.name,
                "arguments": tc.function.arguments or {},
            }
            for tc in tool_calls
        ]

    def chat(self, messages: list[dict]) -> str:
        response = self._client.chat(model=self.model, messages=messages)
        return response.message.content or "No response returned."

    def chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> tuple[str | None, list[dict], dict | None]:
        response = self._client.chat(
            model=self.model,
            messages=messages,
            tools=self._to_ollama_tools(tools),
        )
        msg = response.message

        if msg.tool_calls:
            normalized = self._normalize_tool_calls(msg.tool_calls)
            assistant_msg = {"role": "assistant", "content": msg.content or "", "tool_calls": msg.tool_calls}
            return None, normalized, assistant_msg

        return msg.content or "No response returned.", [], None

    def make_tool_result_message(self, tool_name: str, result: str) -> dict:
        return {"role": "tool", "content": result, "name": tool_name}

    # streaming
    def stream_chat(self, messages: list[dict]) -> Iterator[str]:
        for chunk in self._client.chat(model=self.model, messages=messages, stream=True):
            if chunk.message.content:
                yield chunk.message.content

    def stream_chat_with_tools(
        self,
        messages: list[dict],
        tools: list[dict],
    ) -> Iterator[tuple]:
        """
        Stream a response with tools. Yields:
          ("chunk", text)                              — partial text token
          ("tool_calls", list[dict], assistant_msg)   — model chose to call tools
        Tool calls, if any, are emitted as a single event at the end of the stream.
        """
        ollama_tools = self._to_ollama_tools(tools)
        content_parts: list[str] = []
        final_tool_calls = None

        for chunk in self._client.chat(
            model=self.model,
            messages=messages,
            tools=ollama_tools,
            stream=True,
        ):
            msg = chunk.message
            if msg.content:
                content_parts.append(msg.content)
                yield ("chunk", msg.content)
            if msg.tool_calls:
                # Tool calls arrive on the final chunk; capture and emit after the loop
                final_tool_calls = msg.tool_calls

        if final_tool_calls:
            normalized = self._normalize_tool_calls(final_tool_calls)
            assistant_msg = {
                "role": "assistant",
                "content": "".join(content_parts),
                "tool_calls": final_tool_calls,
            }
            yield ("tool_calls", normalized, assistant_msg)
