import json
from itertools import count
from typing import Iterator

from fastapi import APIRouter, Depends, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel

from config import SYSTEM_PROMPT, STREAMING
from dependencies import get_mcp_client, get_provider
from providers.AbstractProvider import ChatProvider
from tools.confirmation import requires_confirmation
from tools.mcp_client import McpClient

router = APIRouter()

# Module-level counter so tool call IDs are unique across requests.
# Ollama falls back to the tool name as its ID, which breaks when the same
# tool is called twice in one turn — so we assign our own IDs here.
_tc_counter = count(1)


class ChatRequest(BaseModel):
    messages: list[dict]  # [{role: "user"|"assistant", content: "..."}]
    stream: bool = STREAMING


class ChatResponse(BaseModel):
    response: str


class ToolRequest(BaseModel):
    name: str
    arguments: dict


def _run_agentic_loop(
    messages: list[dict],
    provider: ChatProvider,
    mcp: McpClient,
) -> Iterator[tuple]:
    """
    Core agentic loop. Yields tuples that map directly to the frontend wire protocol:

      ("chunk",                text)                     — incremental text token
      ("tool_calls",           [{id, name, arguments}])  — model requested tools (before execution)
      ("tool_result",          id, result)               — result for a single tool call
      ("reasoning",            text)                     — incremental reasoning token (provider-dependent)
      ("confirmation_required", [id, ...])               — listed tool calls need user approval; stream ends

    When any requested tool requires confirmation the loop emits
    ("confirmation_required", ids) and returns without running those tools. The
    frontend approves/declines, runs approved tools via /agent/tool, and
    re-invokes this endpoint with the results carried in the message history.
    """
    tools = mcp.list_tools()

    while True:
        had_tool_calls = False

        for event in provider.stream_chat_with_tools(messages, tools):
            if event[0] == "chunk":
                yield ("chunk", event[1])
            elif event[0] == "reasoning":
                yield ("reasoning", event[1])
            elif event[0] == "tool_calls":
                _, tool_calls, assistant_msg = event
                messages.append(assistant_msg)

                # Assign stable unique IDs before emitting so tool_calls and
                # tool_result events share the same ID the frontend tracks.
                calls = [
                    {"id": f"tc_{next(_tc_counter)}", "name": tc["name"], "arguments": tc["arguments"]}
                    for tc in tool_calls
                ]
                yield ("tool_calls", calls)

                # Run tools that don't need confirmation immediately; collect any
                # that do. A gated call is left without a result here so the
                # frontend can render a confirmation UI for it.
                gated = []
                for call in calls:
                    if requires_confirmation(call["name"]):
                        gated.append(call)
                        continue
                    result = mcp.call_tool(call["name"], call["arguments"])
                    yield ("tool_result", call["id"], result)
                    messages.append(provider.make_tool_result_message(call["name"], result))

                if gated:
                    # Pause the turn and hand control to the frontend. It runs the
                    # approved tools via /agent/tool and re-invokes /agent/chat with
                    # the results in the history, so the loop simply continues there.
                    yield ("confirmation_required", [c["id"] for c in gated])
                    return

                had_tool_calls = True

        if not had_tool_calls:
            break


# Endpoint
@router.post("/agent/chat")
def chat(
    req: ChatRequest,
    provider: ChatProvider = Depends(get_provider),
    mcp: McpClient = Depends(get_mcp_client),
):
    """
    Run the agentic loop. Set stream=true to receive NDJSON events (one JSON array per line),
    or stream=false to wait for the full text response as JSON.
    """
    messages = [{"role": "system", "content": SYSTEM_PROMPT}] + req.messages

    if req.stream:
        def serialised() -> Iterator[str]:
            for event in _run_agentic_loop(messages, provider, mcp):
                yield json.dumps(event) + "\n"

        return StreamingResponse(serialised(), media_type="application/x-ndjson")

    text = "".join(ev[1] for ev in _run_agentic_loop(messages, provider, mcp) if ev[0] == "chunk")
    return ChatResponse(response=text)


@router.post("/agent/tool")
def run_tool(
    req: ToolRequest,
    mcp: McpClient = Depends(get_mcp_client),
):
    """
    Execute a single MCP tool. Called by the frontend to run a tool *after* the
    user has approved it in the confirmation UI (see _run_agentic_loop).

    Restricted to tools in the confirmation registry: this endpoint exists only
    to run gated tools post-approval, so anything else is rejected.
    """
    if not requires_confirmation(req.name):
        raise HTTPException(status_code=403, detail=f"Tool '{req.name}' is not confirmable")
    return {"result": mcp.call_tool(req.name, req.arguments)}
