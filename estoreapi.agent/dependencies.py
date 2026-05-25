from fastapi import Request

from providers.AbstractProvider import ChatProvider
from tools.executor import Executor


# FastAPI dependency functions
# Services are initialised once during app lifespan (see main.py) and stored
# on app.state so all routers share the same instances.

def get_provider(request: Request) -> ChatProvider:
    return request.app.state.provider


def get_executor(request: Request) -> Executor:
    return request.app.state.executor
