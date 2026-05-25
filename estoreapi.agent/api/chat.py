import json
from pathlib import Path
from typing import Iterator

from fastapi import APIRouter, Depends
from fastapi.responses import StreamingResponse
from pydantic import BaseModel

from config import SYSTEM_PROMPT, STREAMING
from dependencies import get_executor, get_provider
from providers.AbstractProvider import ChatProvider
from tools.executor import Executor

router = APIRouter()

_tools_path = Path(__file__).parent.parent / "tools" / "definitions.json"
with open(_tools_path) as f:
    TOOLS: list[dict] = json.load(f)


class ChatRequest(BaseModel):
    messages: list[dict]  # [{role: "user"|"assistant", content: "..."}]
    stream: bool = STREAMING


class ChatResponse(BaseModel):
    response: str


# streaming & non-streaming compatible
def _run_agentic_loop(
    messages: list[dict],
    provider: ChatProvider,
    executor: Executor,
) -> Iterator[str]:
    """
    Core loop: yields text chunks as they arrive, executing any tool calls
    transparently between iterations.
    """
    while True:
        had_tool_calls = False

        for event in provider.stream_chat_with_tools(messages, TOOLS):
            if event[0] == "chunk":
                yield event[1]
            elif event[0] == "tool_calls":
                _, tool_calls, assistant_msg = event
                # Append the assistant's tool-call message, then each tool result
                messages.append(assistant_msg)
                for tc in tool_calls:
                    result = executor.dispatch(tc["name"], tc["arguments"])
                    messages.append(provider.make_tool_result_message(tc["name"], result))
                had_tool_calls = True

        if not had_tool_calls:
            break


# Endpoint
@router.post("/agent/chat")
def chat(
    req: ChatRequest,
    provider: ChatProvider = Depends(get_provider),
    executor: Executor = Depends(get_executor),
):
    """
    Run the agentic loop. Tool calls execute transparently; only the final text
    is returned. Set stream=true to receive chunks as they arrive (text/plain),
    or stream=false (default) to wait for the full response (application/json).
    """
    messages = [{"role": "system", "content": SYSTEM_PROMPT}] + req.messages

    if req.stream:
        return StreamingResponse(
            _run_agentic_loop(messages, provider, executor),
            media_type="text/plain",
        )

    # Non-streaming: collect all chunks then return JSON
    text = "".join(_run_agentic_loop(messages, provider, executor))
    return ChatResponse(response=text)