from memory.user_context import user_id_var


def make_memory_search_handler(memory):
    def memory_search(query: str) -> str:
        return memory.search(query, user_id_var.get())
    return memory_search
