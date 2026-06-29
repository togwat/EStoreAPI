"""
Keep a model registry from models.dev/api.json
Can get model metadata such as the context window
"""
import json
from functools import lru_cache
from urllib.request import Request, urlopen


@lru_cache(maxsize=1)
def _registry() -> dict:
    """The models.dev database, fetched once per process (result memoized, failures included)."""
    try:
        req = Request("https://models.dev/api.json", headers={"User-Agent": "estoreapi-agent"})
        with urlopen(req, timeout=10) as resp:
            return json.load(resp)
    except Exception:
        return {}


def get_context_window(provider_id: str, model_id: str) -> int | None:
    """Look up the model's context window on models.dev, or None if it isn't listed."""
    try:
        return _registry()[provider_id]["models"][model_id]["limit"]["context"]
    except (KeyError, TypeError):
        return None