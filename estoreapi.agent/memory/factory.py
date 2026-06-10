from config import MEMORY_ENABLED, MEM0_DB_HOST, MEM0_DB_PORT, MEM0_DB_USER, MEM0_DB_PASSWORD, MEM0_LLM_MODEL, MEM0_EMBEDDING_MODEL, OLLAMA_HOST
from memory.Mem0MemoryProvider import Mem0MemoryProvider


def create_memory() -> Mem0MemoryProvider | None:
    """
    Instantiate the configured memory provider, or return None if memory is disabled.
    Add new providers here as they are implemented.
    """
    if not MEMORY_ENABLED:
        return None

    # default to mem0 for now
    return Mem0MemoryProvider(
        ollama_host=OLLAMA_HOST,
        llm_model=MEM0_LLM_MODEL,
        embedding_model=MEM0_EMBEDDING_MODEL,
        db_host=MEM0_DB_HOST,
        db_port=MEM0_DB_PORT,
        db_user=MEM0_DB_USER,
        db_password=MEM0_DB_PASSWORD,
    )
