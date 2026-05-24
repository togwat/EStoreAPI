import json
from pathlib import Path

from fastapi import APIRouter, Depends
from pydantic import BaseModel

from config import SYSTEM_PROMPT
from dependencies import get_executor, get_provider
from providers.AbstractProvider import ChatProvider
from tools.executor import Executor

router = APIRouter()

# Tool definitions
_tools_path = Path(__file__).parent.parent / "tools" / "definitions.json"
with open(_tools_path) as f:
    TOOLS: list[dict] = json.load(f)


class ChatRequest(BaseModel):
    messages: list[dict]  # [{role: "user"|"assistant", content: "..."}]


class ChatResponse(BaseModel):
    response: str


# Endpoint
@router.post("/agent/chat", response_model=ChatResponse)
def chat(
    req: ChatRequest,
    provider: ChatProvider = Depends(get_provider),
    executor: Executor = Depends(get_executor),
) -> ChatResponse:
    """Run the agentic loop and return the model's final text response."""
    messages = [{"role": "system", "content": SYSTEM_PROMPT}] + req.messages

    while True:
        text, tool_calls, assistant_msg = provider.chat_with_tools(messages, TOOLS)

        # Model responded with plain text — loop complete
        if not tool_calls:
            return ChatResponse(response=text or "")

        # Append the assistant's tool-call message, then each tool result
        messages.append(assistant_msg)
        for tc in tool_calls:
            result = executor.dispatch(tc["name"], tc["arguments"])
            messages.append(provider.make_tool_result_message(tc["name"], result))
        # Loop: model now sees the tool results and responds again