from config import CHAT_DB_HOST, CHAT_DB_PORT, CHAT_DB_USER, CHAT_DB_PASSWORD, CHAT_DB_NAME
from store.AbstractChatStore import AbstractChatStore
from store.ChatStore import ChatStore


def create_chat_store() -> AbstractChatStore:
    """
    Instantiate the configured chat store.
    Add new store backends here as they are implemented.
    """
    return ChatStore(
        host=CHAT_DB_HOST,
        port=CHAT_DB_PORT,
        user=CHAT_DB_USER,
        password=CHAT_DB_PASSWORD,
        dbname=CHAT_DB_NAME,
    )