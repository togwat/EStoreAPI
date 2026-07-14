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
        {name, description}
        """
        pass

    @abstractmethod
    def get_skill(self, name: str) -> str:
        """Retrieve the skill document with the given name"""
        pass

    @abstractmethod
    def create_skill(self, name: str, description: str, content: str) -> str:
        """
        Create a skill document.

        name: the unique id of the skill, used for retrieval
        description: short summary of the skill that is always fed to the agent, so it knows when to get this skill.
        content: hidden to the agent until retrieved
        Returns a confirmation message.
        """
        pass

    @abstractmethod
    def update_skill(self, name: str, description: str | None = None, content: str | None = None) -> str:
        """
        Update a skill's description or content. If either are empty/none, the fields stay as-is.
        Returns a confirmation message.
        """
        pass

    @abstractmethod
    def delete_skill(self, name: str) -> str:
        """
        Deletes the skill with the given name.
        Returns a confirmation message.
        """
        pass
