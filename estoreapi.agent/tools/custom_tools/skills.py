from collections.abc import Callable

from skills.SkillProvider import SkillProvider


def make_skills_handler(provider: SkillProvider) -> dict[str, Callable[..., str]]:
    def _unknown_skill(name: str) -> str:
        """Miss message that lists what does exist, so the model can self-correct."""
        names = [skill["name"] for skill in provider.list_skills()]

        if not names:
            return f"Unknown skill '{name}'. No skills are saved yet."

        return f"Unknown skill '{name}'. Available skills: {', '.join(names)}"

    def get_skill(name: str) -> str:
        skill = provider.get_skill(name)

        if skill is None:
            return _unknown_skill(name)

        provider.record_use(name)

        return f"# {name}\n\n{skill['content']}"

    def create_skill(name: str, summary: str, content: str) -> str:
        if not provider.create_skill(name, summary, content):
            return f"Skill '{name}' already exists. Use update_skill to modify it, or pick a different name."

        return f"Skill '{name}' created."

    def update_skill(name: str, summary: str | None = None, content: str | None = None) -> str:
        if not summary and not content:
            return "Nothing to update: provide summary and/or content."

        if not provider.update_skill(name, summary, content):
            return _unknown_skill(name)

        return f"Skill '{name}' updated."

    def delete_skill(name: str) -> str:
        if not provider.delete_skill(name):
            return _unknown_skill(name)

        return f"Skill '{name}' deleted."

    return {
        "get_skill": get_skill,
        "create_skill": create_skill,
        "update_skill": update_skill,
        "delete_skill": delete_skill,
    }
