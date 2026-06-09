from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from api.chat import router as chat_router
from config import MCP_URL
from memory.factory import create_memory
from providers.factory import create_provider
from tools.descriptions.JsonDescriptionService import JsonDescriptionService
from tools.AbstractToolClient import AbstractToolClient
from tools.mcp.CorrectingMcpClient import CorrectingMcpClient
from tools.custom_tools.CustomToolClient import CustomToolClient
from tools.custom_tools.registry import Registry
from tools.router import ToolRouter


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Initialise shared services on startup; clean up on shutdown."""

    memory = create_memory()

    desc_service = JsonDescriptionService()
    # custom tools definition reg
    registry = Registry(desc_service=desc_service)

    # Mutable reference populated after clients are built so the lazy callable
    # captures the full client list without a circular dependency at construction time.
    _clients: list[AbstractToolClient] = []

    def get_all_tools() -> list[dict]:
        return [t for c in _clients for t in c.list_tools()]

    clients: list[AbstractToolClient] = [
        CustomToolClient(registry=registry, desc_service=desc_service, memory=memory, get_all_tools=get_all_tools),
        CorrectingMcpClient(url=MCP_URL, desc_service=desc_service),
    ]
    _clients.extend(clients)

    app.state.memory = memory
    app.state.provider = create_provider()
    app.state.clients = clients
    app.state.router = ToolRouter(clients=clients)
    app.state.get_all_tools = get_all_tools
    
    yield


app = FastAPI(title="EStore Agent", lifespan=lifespan)

# Allow the Vite dev server to reach us directly when not going via the proxy
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173"],
    allow_methods=["POST"],
    allow_headers=["Content-Type"],
)

app.include_router(chat_router)
