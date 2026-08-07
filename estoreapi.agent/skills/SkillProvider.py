from abc import ABC, abstractmethod


# CRUD on a skill repo
class SkillProvider(ABC):
    @abstractmethod
    def init_schema(self) -> None:
        """Prepare skill storage (folder for md files, db, etc.)"""
        pass

    @abstractmethod
    def list_skills(self) -> list[dict]:
        """
        Returns a list of all skills in the format:
        {name, summary}
        """
        pass

    @abstractmethod
    def get_skill(self, name: str) -> dict | None:
        """
        Retrieve the skill document with the given name in the format:
        {name, summary, content}
        """
        pass

    @abstractmethod
    def create_skill(self, name: str, summary: str, content: str) -> bool:
        """
        Create a skill document.

        name: the unique id of the skill, used for retrieval
        summary: short summary of the skill that is always fed to the agent, so it knows when to get this skill.
        content: hidden to the agent until retrieved

        Returns False if the name is already taken.
        """
        pass

    @abstractmethod
    def update_skill(self, name: str, summary: str | None = None, content: str | None = None) -> bool:
        """
        Update a skill's summary or content. If either are empty/none, the fields stay as-is.

        Returns False if no skill has that name.
        """
        pass

    @abstractmethod
    def delete_skill(self, name: str) -> bool:
        """
        Deletes the skill with the given name.

        Returns False if no skill has that name.
        """
        pass

    @abstractmethod
    def record_use(self, name: str) -> None:
        """
        Increment the skill's usa count by one and set last used time to present time.
        For use when the agent calls get_skill.
        """
        pass
