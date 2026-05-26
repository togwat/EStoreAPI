import os
from pathlib import Path

from dotenv import load_dotenv

load_dotenv(Path(__file__).parent.parent / ".env")

# Provider
PROVIDER = os.environ["PROVIDER"]
OLLAMA_HOST = os.environ["OLLAMA_HOST"]
OLLAMA_MODEL = os.environ["OLLAMA_MODEL"]

# ASP.NET
API_BASE_URL = os.environ["API_BASE_URL"]
MCP_URL = os.getenv("MCP_URL", f"{API_BASE_URL}/mcp")

SYSTEM_PROMPT = (
    "You are a phone repair shop (E-Store) management assistant."
    "Use the available tools to read from and write to the database."
    "Before performing any tool call that create, modify, or delete data, identify all required fields from the schema. If any required fields are missing or ambiguous, ask the user to clarify and list all missing fields at once. Do not infer, default, or fabricate any field values. Once all required data is confirmed, summarize what will be written and ask for explicit user approval before executing."
)

STREAMING = True