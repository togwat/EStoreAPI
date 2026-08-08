from abc import ABC, abstractmethod


# CRUD on a skill repo
# is assumed to be handling md files

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
    def get_skill(self, name: str) -> str | None:
        """
        Retrieve the skill document with the given name.

        Returns None if no skill has that name.
        """
        pass

    @abstractmethod
    def create_skill(self, name: str, file: str) -> bool:
        """
        Create a skill document.

        name: the unique id of the skill, used for retrieval. Doubles as the file name.
        file: the markdown file, whose frontmatter must hold the summary.
        Content is hidden to the agent until retrieved.

        Returns False if the name is already taken.
        Raises ValueError if the document is not in the expected format.
        """
        pass

    @abstractmethod
    def update_skill(self, name: str, file: str) -> bool:
        """
        Update a skill by overwriting its document.

        Returns False if no skill has that name.
        Raises ValueError if the document is not in the expected format.
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
        Increment the skill's use count by one and set last used time to present time.
        For use when the agent calls get_skill.
        """
        pass
