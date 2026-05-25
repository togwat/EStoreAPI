from config import OLLAMA_HOST, OLLAMA_MODEL, PROVIDER
from providers.AbstractProvider import ChatProvider


def create_provider() -> ChatProvider:
    """
    Instantiate the configured ChatProvider.
    Add new providers here as they are implemented.
    """
    if PROVIDER == "ollama":
        from providers.OllamaProvider import OllamaProvider
        return OllamaProvider(model=OLLAMA_MODEL, host=OLLAMA_HOST)

    raise ValueError(f"Unknown provider '{PROVIDER}'. Set the PROVIDER env var to a supported value.")
