from collections.abc import Callable

from skills.SkillProvider import SkillProvider
from skills.markdown_parser import from_markdown, to_markdown


def make_skills_handler(provider: SkillProvider) -> dict[str, Callable[..., str]]:
    def _unknown_skill(name: str) -> str:
        """Miss message that lists what does exist, so the model can self-correct."""
        names = [skill["name"] for skill in provider.list_skills()]

        if not names:
            return f"Unknown skill '{name}'. No skills are saved yet."

        return f"Unknown skill '{name}'. Available skills: {', '.join(names)}"

    def get_skill(name: str) -> str:
        file = provider.get_skill(name)

        if file is None:
            return _unknown_skill(name)

        provider.record_use(name)

        # Return content only
        return from_markdown(name, file)["content"]

    def create_skill(name: str, summary: str, content: str) -> str:
        file = to_markdown({"name": name, "summary": summary, "content": content})

        if not provider.create_skill(name, file):
            return f"Skill '{name}' already exists. Use update_skill to modify it, or pick a different name."

        return f"Skill '{name}' created."

    def update_skill(name: str, summary: str | None = None, content: str | None = None) -> str:
        if not summary and not content:
            return "Nothing to update: provide summary and/or content."

        current = provider.get_skill(name)
        if current is None:
            return _unknown_skill(name)

        # SkillProvider replace the whole file, so do a merge onto the current file
        skill = from_markdown(name, current)
        skill["summary"] = summary or skill["summary"]
        skill["content"] = content or skill["content"]

        if not provider.update_skill(name, to_markdown(skill)):
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
