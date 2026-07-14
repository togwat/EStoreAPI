from config import CHAT_DB_HOST, CHAT_DB_PORT, CHAT_DB_USER, CHAT_DB_PASSWORD, CHAT_DB_NAME
from skills.SkillProvider import SkillProvider
from skills.DbSkillProvider import DbSkillProvider


def create_skill_provider() -> SkillProvider:
    """
    Skills live in the same database as ChatStore, as a separate table.
    """
    return DbSkillProvider(
        host=CHAT_DB_HOST,
        port=CHAT_DB_PORT,
        user=CHAT_DB_USER,
        password=CHAT_DB_PASSWORD,
        dbname=CHAT_DB_NAME,
    )