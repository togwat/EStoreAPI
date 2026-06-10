from memory.user_context import user_id_var
from config import MEMORY_ENABLED


def make_memory_search_handler(memory):
    def memory_search(query: str) -> str:
        if MEMORY_ENABLED:
            return memory.search(query, user_id_var.get())
        else:
            return "Memory is not enabled."
    return memory_search
