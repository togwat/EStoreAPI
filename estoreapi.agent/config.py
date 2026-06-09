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

SYSTEM_PROMPT = (
    "You are a phone repair shop (E-Store) management assistant."
    "Use the available tools to read from and write to the database."
    "Never fabricate, infer, or guess any data, including names, IDs, prices, or any other field values regardless of whether you are reading or writing. If information is not present in a tool result, check the descriptions of other tools for potential solutions. Otherwise, state that information cannot be retrieved with the available tools, and present your current data as-is and explain what information is absent."
    "Before performing any tool call that creates, modifies, or deletes data, identify all required fields from the schema. If any required fields' values are missing or ambiguous, ask the user to clarify and list all missing fields at once. Once all required data is confirmed, summarise what will be written and ask for explicit user approval before executing."
)

STREAMING = True