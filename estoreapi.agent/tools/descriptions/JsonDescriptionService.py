import json
from pathlib import Path

from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService

_DEFAULT_PATH = Path(__file__).parent / "descriptions.json"


class JsonDescriptionService(AbstractDescriptionService):
    """
    JSON-backed description store
    """

    def __init__(self, path: Path = _DEFAULT_PATH):
        self._path = path

        # create file if missing
        if not self._path.exists():
            self._path.parent.mkdir(parents=True, exist_ok=True)
            self._path.write_text("{}", encoding="utf-8")

    def get(self, tool_name: str) -> str | None:
        """
        If a tool name has no entry yet, get() inserts an empty string and saves,
        creating the file if needed. Empty strings are falsy, so callers using
        `get() or default` fall back to the default description automatically.
        """
        data = self._load()
        if tool_name not in data:
            # Seed an empty placeholder so the key appears in the file.
            data[tool_name] = ""
            self._save(data)
        return data[tool_name] or None

    def update(self, tool_name: str, description: str) -> str:
        data = self._load()
        data[tool_name] = description
        self._save(data)
        return f"Updated description for '{tool_name}'"

    def _load(self) -> dict[str, str]:
        if self._path.exists():
            text = self._path.read_text(encoding="utf-8").strip()
            if text:
                return json.loads(text)
        return {}

    def _save(self, data: dict[str, str]) -> None:
        self._path.write_text(
            json.dumps(data, indent=2, ensure_ascii=False),
            encoding="utf-8",
        )
