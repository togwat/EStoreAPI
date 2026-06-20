import json
from providers.AbstractProvider import ChatProvider
from typing import Iterator
from openai import OpenAI


class DeepseekProvider(ChatProvider):
    def __init__(self, model, host, key):
        self.model = model
        self.host = host
        # Deepseek can use OpenAI api
        self._client = OpenAI(api_key=key, base_url=host)
        print(f"Provider Deepseek connected at {host}. Model: {self.model}")

    def stream_chat_with_tools(self, messages: list[dict], tools: list[dict]) \
    -> Iterator[tuple]:
        """
        Stream a response with tools. Yields:
          ("reasoning", text)                          — partial reasoning/thinking token
          ("chunk", text)                              — partial text token
          ("tool_calls", list[dict], assistant_msg)   — model chose to call tools
        Tool calls, if any, are emitted as a single event at the end of the stream.
        """
        content_parts: list[str] = []
        # Deepseek's thinking mode requires the reasoning that accompanied a tool call to be
        # passed back on the next request, so accumulate it alongside the content.
        reasoning_parts: list[str] = []
        # OpenAI streams tool calls as fragments keyed by index: the name arrives once and
        # the JSON arguments are delivered in pieces, so accumulate per index until the end.
        tool_acc: dict[int, dict] = {}

        for chunk in self._client.chat.completions.create(
            model=self.model,
            messages=self._normalise_messages(messages),  # type: ignore[arg-type]
            tools=self._to_openai_tools(tools),  # type: ignore[arg-type]
            stream=True,
            reasoning_effort="high",
            parallel_tool_calls=False,
            extra_body={"thinking": {"type": "enabled"}},
        ):
            if not chunk.choices:
                continue
            delta = chunk.choices[0].delta

            # Deepseek-specific: reasoning tokens arrive on reasoning_content
            reasoning = getattr(delta, "reasoning_content", None)
            if reasoning:
                reasoning_parts.append(reasoning)
                yield ("reasoning", reasoning)
            if delta.content:
                content_parts.append(delta.content)
                yield ("chunk", delta.content)
            if delta.tool_calls:
                for tc in delta.tool_calls:
                    entry = tool_acc.setdefault(tc.index, {"id": "", "name": "", "arguments": ""})
                    if tc.id:
                        entry["id"] = tc.id
                    if tc.function and tc.function.name:
                        entry["name"] = tc.function.name
                    if tc.function and tc.function.arguments:
                        entry["arguments"] += tc.function.arguments

        if tool_acc:
            normalised = self._normalise_tool_calls(tool_acc)
            assistant_msg = {
                "role": "assistant",
                "content": "".join(content_parts) or None,
                "tool_calls": [
                    {
                        "id": call["id"],   # unique per call so parallel calls to the same tool don't collide
                        "type": "function",
                        "function": {
                            "name": call["name"],
                            "arguments": json.dumps(call["arguments"]),
                        },
                    }
                    for call in normalised
                ],
            }
            if reasoning_parts:
                # Required by thinking mode when this message is replayed on the next request.
                assistant_msg["reasoning_content"] = "".join(reasoning_parts)
            yield ("tool_calls", normalised, assistant_msg)

    def make_tool_result_message(self, tool_call_id: str, result: str) -> dict:
        # OpenAI requires tool_call_id to match the assistant's tool_calls[].id exactly, so
        # results bind to the right call even when the same tool is called multiple times.
        return {"role": "tool", "tool_call_id": tool_call_id, "content": result}

    def _to_openai_tools(self, tools: list[dict]) -> list[dict]:
        """Convert MCP tool format (input_schema) to OpenAI's expected format (parameters)."""
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

    def _normalise_tool_calls(self, tool_acc: dict[int, dict]) -> list[dict]:
        """Normalise accumulated streamed tool calls to {id, name, arguments} dicts."""
        return [
            {
                # Deepseek supplies a unique id per call; fall back to the stream index if absent.
                "id": entry["id"] or f"call_{idx}",
                "name": entry["name"],
                "arguments": json.loads(entry["arguments"] or "{}"),
            }
            for idx, entry in tool_acc.items()
        ]

    def _normalise_messages(self, messages: list[dict]) -> list[dict]:
        """
        Convert the provider-agnostic content format to OpenAI's native format.

        The frontend sends content as a typed parts list which may include:
          {"type": "text", "text": "..."}
          {"type": "image_url", "url": "data:image/png;base64,..."}
          {"type": "tool_use", "id": "...", "name": "...", "input": {...}, "result": ...}

        OpenAI expects either a plain string or a list of content parts (with images carried
        as {"type": "image_url", "image_url": {"url": ...}}), and tool calls as a dedicated
        "tool_calls" key. An assistant message with tool_use parts is expanded into an
        assistant message (with tool_calls) followed by one tool message per result.
        """
        result = []
        for msg in messages:
            result.extend(self._expand_message(msg))
        return result

    def _expand_message(self, msg: dict) -> list[dict]:
        content = msg.get("content")
        if not isinstance(content, list):
            # Already-native messages (system prompt, in-loop assistant/tool messages) pass through.
            return [msg]

        text = " ".join(p["text"] for p in content if p.get("type") == "text")
        reasoning = "".join(p["text"] for p in content if p.get("type") == "reasoning")
        images = [
            p["url"]
            for p in content
            if p.get("type") == "image_url" and p.get("url")
        ]
        tool_uses = [p for p in content if p.get("type") == "tool_use"]

        if not tool_uses:
            if images:
                parts = [{"type": "text", "text": text}] if text else []
                parts += [{"type": "image_url", "image_url": {"url": url}} for url in images]
                return [{**msg, "content": parts}]
            return [{**msg, "content": text}]

        # Assistant message with tool calls: emit assistant msg then tool result messages.
        assistant_msg: dict = {
            "role": "assistant",
            "content": text or None,
            "tool_calls": [
                {
                    "id": p["id"],
                    "type": "function",
                    "function": {"name": p["name"], "arguments": json.dumps(p["input"])},
                }
                for p in tool_uses
            ],
        }
        if reasoning:
            # Send the reasoning back with its tool call
            assistant_msg["reasoning_content"] = reasoning

        # Cancelled mid-generation means no content
        tool_msgs = [
            {
                "role": "tool",
                "tool_call_id": p["id"],
                "content": str(p["result"]) if p.get("result") is not None else "[cancelled — no result]"
            }
            for p in tool_uses
        ]

        return [assistant_msg] + tool_msgs