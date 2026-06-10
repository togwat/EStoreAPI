from mem0 import Memory

from memory.MemoryProvider import MemoryProvider


class Mem0MemoryProvider(MemoryProvider):
    def __init__(
        self,
        ollama_host: str,
        llm_model: str,
        embedding_model: str,
        db_host: str,
        db_port: int,
        db_user: str,
        db_password: str,
    ):
        config = {
            "llm": {
                "provider": "ollama",
                "config": {
                    "model": llm_model,
                    "temperature": 0,
                    "max_tokens": 2000,
                    "ollama_base_url": ollama_host,
                },
            },
            "embedder": {
                "provider": "ollama",
                "config": {
                    "model": embedding_model,
                    "ollama_base_url": ollama_host,
                },
            },
            "vector_store": {
                "provider": "pgvector",
                "config": {
                    "host": db_host,
                    "port": db_port,
                    "user": db_user,
                    "password": db_password,
                    "dbname": "postgres",
                    # nomic-embed-text produces 768-dim vectors; mismatching this with the
                    # pgvector default (1536) causes schema errors on first insert.
                    "embedding_model_dims": 768,
                },
            },
        }
        self._memory = Memory.from_config(config)

    def get_context(self, user_id: str) -> str:
        """
        Returns standing context that is broadly relevant across all conversations,
        injected once into the system prompt at session start.
        """
        result = self._memory.search(
            "user identity preferences standing instructions",
            filters={"user_id": user_id},
        )
        memories = result.get("results", [])
        if not memories:
            return ""
        return "Persistent context:\n" + "\n".join(f"- {m['memory']}" for m in memories)

    
    @staticmethod
    def _extract_text(content: str | list) -> str:
        if isinstance(content, list):
            return " ".join(b["text"] for b in content if b.get("type") == "text" and b.get("text"))
        return content

    @staticmethod
    def _text_only(msg: dict) -> dict | None:
        """
        Normalise a message for mem0. Providers like Anthropic return assistant content
        as a list of typed blocks (text, tool_use, tool_result). mem0's vision parser
        crashes on non-text blocks when no vision LLM is configured, so we extract only
        the text parts and drop messages that have no text at all (e.g. pure tool calls).
        """
        text = Mem0MemoryProvider._extract_text(msg["content"])
        
        if not text:
            return None
        return msg if isinstance(msg["content"], str) else {"role": msg["role"], "content": text}

    def write(self, messages: list[dict], user_id: str) -> None:
        """
        Submits the completed conversation turn to mem0 for fact extraction.

        mem0 runs an LLM over the messages to extract durable facts worth remembering
        (preferences, names, context) and discards transient details. Only user/assistant
        pairs are passed, with content normalised to plain text.
        """
        conversation = [
            normalised
            for m in messages
            if m["role"] in ("user", "assistant")
            for normalised in (self._text_only(m),)
            if normalised is not None
        ]
        if conversation:
            self._memory.add(conversation, user_id=user_id)

    def search(self, query: str | list, user_id: str) -> str:
        """
        Semantic search over stored memories for a specific query.

        Used as a callable tool mid-conversation when the model needs to recall something
        specific.
        Returns the top matching facts ranked by semantic similarity score.
        """
        query = self._extract_text(query)
        if not query:
            return "No relevant memories found."
        result = self._memory.search(query, filters={"user_id": user_id})
        results = result.get("results", [])
        if not results:
            return "No relevant memories found."
        return "Relevant memories:\n" + "\n".join(f"- {r['memory']}" for r in results)
