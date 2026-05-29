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
        """Return the override for the given key, or None if not set."""
        data = self._load()
        return data.get(tool_name) or None

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
