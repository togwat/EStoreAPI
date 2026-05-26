from fastapi import Request

from providers.AbstractProvider import ChatProvider
from tools.mcp_client import McpClient


# FastAPI dependency functions
# Services are initialised once during app lifespan (see main.py) and stored
# on app.state so all routers share the same instances.

def get_provider(request: Request) -> ChatProvider:
    return request.app.state.provider

def get_mcp_client(request: Request) -> McpClient:
    return request.app.state.mcp
