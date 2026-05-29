from fastapi import Request

from providers.AbstractProvider import ChatProvider
from tools.AbstractToolClient import AbstractToolClient
from tools.router import ToolRouter


# FastAPI dependency functions
# Services are initialised once during app lifespan (see main.py) and stored
# on app.state so all routers share the same instances.

def get_provider(request: Request) -> ChatProvider:
    return request.app.state.provider

def get_clients(request: Request) -> list[AbstractToolClient]:
    return request.app.state.clients

def get_router(request: Request) -> ToolRouter:
    return request.app.state.router