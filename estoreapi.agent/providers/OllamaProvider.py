from providers.AbstractProvider import ChatProvider
from ollama import Client
from typing import Iterator


class OllamaProvider(ChatProvider):
    def __init__(self, model, host):
        self.model = model
        self.host = host
        self._client = Client(host=host)
        print(f"Provider Ollama connected at {host}. Model: {self.model}")

    def stream_chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> Iterator[tuple]:
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
            messages=self._normalize_messages(messages),
            tools=ollama_tools,
            stream=True,
        ):
            msg = chunk.message
            if msg.thinking:
                yield ("reasoning", msg.thinking)
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
            
    def make_tool_result_message(self, tool_name: str, result: str) -> dict:
        return {"role": "tool", "content": result, "name": tool_name}

    def _to_ollama_tools(self, tools: list[dict]) -> list[dict]:
        """Convert MCP tool format (input_schema) to Ollama's expected format (parameters)."""
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

    def _normalize_messages(self, messages: list[dict]) -> list[dict]:
        """
        Convert the provider-agnostic content format to Ollama's native format.

        The frontend sends content as a typed parts list:
          [{"type": "text", "text": "..."}, {"type": "image_url", "url": "data:image/png;base64,..."}]

        Ollama expects a plain string for content and a separate "images" list of raw base64 strings.
        """
        result = []
        for msg in messages:
            content = msg.get("content")
            if isinstance(content, list):
                text = " ".join(p["text"] for p in content if p.get("type") == "text")
                images = [
                    p["url"].split(";base64,", 1)[1]
                    for p in content
                    if p.get("type") == "image_url" and ";base64," in p["url"]
                ]
                msg = {**msg, "content": text}
                if images:
                    msg["images"] = images
            result.append(msg)
        return result
