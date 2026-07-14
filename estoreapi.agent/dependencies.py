from fastapi import HTTPException, Request

from memory.MemoryProvider import MemoryProvider
from providers.AbstractProvider import ChatProvider
from skills.SkillProvider import SkillProvider
from store.AbstractChatStore import AbstractChatStore
from tools.AbstractToolClient import AbstractToolClient
from tools.router import ToolRouter


# FastAPI dependency functions
# Services are initialised once during app lifespan (see main.py) and stored
# on app.state so all routers share the same instances.

def get_provider(request: Request) -> ChatProvider:
    return request.app.state.provider

def get_all_tools(request: Request) -> list[dict]:
    return request.app.state.get_all_tools()

def get_router(request: Request) -> ToolRouter:
    return request.app.state.router

def get_memory(request: Request) -> MemoryProvider | None:
    return request.app.state.memory

def get_store(request: Request) -> AbstractChatStore:
    return request.app.state.store

def get_skills(request: Request) -> SkillProvider:
    return request.app.state.skills

def get_user_email(request: Request) -> str:
    """User identity from the X-User-Email header injected by nginx after auth."""
    email = request.headers.get("x-user-email", "").strip()
    if not email:
        raise HTTPException(status_code=401, detail="Missing user identity header")
    return email