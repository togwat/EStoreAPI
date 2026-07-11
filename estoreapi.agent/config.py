import os
from pathlib import Path

from dotenv import load_dotenv

load_dotenv(Path(__file__).parent.parent / ".env")

# Provider
PROVIDER = os.environ["PROVIDER"]
OLLAMA_HOST = os.getenv("OLLAMA_HOST")
OLLAMA_MODEL = os.getenv("OLLAMA_MODEL")
# other keys
DEEPSEEK_KEY = os.getenv("DEEPSEEK_KEY")
DEEPSEEK_HOST ="https://api.deepseek.com"
DEEPSEEK_MODEL = os.getenv("DEEPSEEK_MODEL", "deepseek-v4-flash")

# ASP.NET
API_BASE_URL = os.environ["API_BASE_URL"]
MCP_URL = os.getenv("MCP_URL", f"{API_BASE_URL}/mcp")

# web search
TAVILY_KEY = os.environ["TAVILY_KEY"]

# memory
MEMORY_ENABLED = os.environ["MEMORY_ENABLED"].lower() == "true"
MEM0_DB_HOST = os.environ["MEM0_DB_HOST"]
MEM0_DB_PORT = int(os.environ["MEM0_DB_PORT"])
MEM0_DB_USER = os.environ["MEM0_DB_USER"]
MEM0_DB_PASSWORD = os.environ["MEM0_DB_PASSWORD"]
MEM0_LLM_MODEL = os.environ["MEM0_LLM_MODEL"]
MEM0_EMBEDDING_MODEL = os.environ["MEM0_EMBEDDING_MODEL"]

# chat history persistence
CHAT_DB_HOST = os.environ["CHAT_DB_HOST"]
CHAT_DB_PORT = int(os.environ["CHAT_DB_PORT"])
CHAT_DB_USER = os.environ["CHAT_DB_USER"]
CHAT_DB_PASSWORD = os.environ["CHAT_DB_PASSWORD"]
CHAT_DB_NAME = os.environ["CHAT_DB_NAME"]

SYSTEM_PROMPT = (Path(__file__).parent / "system.md").read_text(encoding="utf-8")

STREAMING = True