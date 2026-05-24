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

SYSTEM_PROMPT = (
    "You are a phone repair shop (E-Store) management assistant."
    "Use the available tools to read from and write to the database."
    "Respond in natural language. Confirm actions taken."
)

STREAMING = False