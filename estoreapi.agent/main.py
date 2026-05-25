from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from api.chat import router as chat_router
from config import API_BASE_URL
from providers.factory import create_provider
from tools.executor import Executor


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Initialise shared services on startup; clean up on shutdown."""
    app.state.provider = create_provider()
    app.state.executor = Executor(base_url=API_BASE_URL)
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