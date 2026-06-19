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
            
    def make_tool_result_message(self, tool_call_id: str, result: str) -> dict:
        return {"role": "tool", "content": result, "name": tool_call_id}

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

        The frontend sends content as a typed parts list which may include:
          {"type": "text", "text": "..."}
          {"type": "image_url", "url": "data:image/png;base64,..."}
          {"type": "tool_use", "id": "...", "name": "...", "input": {...}, "result": ...}

        Ollama expects plain string content, a separate "images" list, and tool calls as a
        dedicated "tool_calls" key. An assistant message with tool_use parts is expanded
        into an assistant message (with tool_calls) followed by one tool message per result.
        """
        result = []
        for msg in messages:
            result.extend(self._expand_message(msg))
        return result

    def _expand_message(self, msg: dict) -> list[dict]:
        content = msg.get("content")
        if not isinstance(content, list):
            return [msg]
        
        text = " ".join(p["text"] for p in content if p.get("type") == "text")
        images = [
            p["url"].split(";base64,", 1)[1]
            for p in content
            if p.get("type") == "image_url" and ";base64," in p["url"]
        ]
        tool_uses = [p for p in content if p.get("type") == "tool_use"]

        if not tool_uses:
            normalized = {**msg, "content": text}
            if images:
                normalized["images"] = images
            return [normalized]

        # Assistant message with tool calls: emit assistant msg then tool result messages.
        assistant_msg: dict = {
            "role": "assistant",
            "content": text,
            "tool_calls": [
                {"function": {"name": p["name"], "arguments": p["input"]}}
                for p in tool_uses
            ],
        }
        if images:
            assistant_msg["images"] = images

        # Cancelled mid-generation means no content
        tool_content = str(p["result"]) if p.get("result") is not None else "[cancelled — no result]"
        tool_msgs = [
            {"role": "tool", "content": tool_content, "name": p["name"]}
            for p in tool_uses
        ]

        return [assistant_msg] + tool_msgs
