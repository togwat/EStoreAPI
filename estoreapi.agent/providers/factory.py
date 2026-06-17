from config import OLLAMA_HOST, OLLAMA_MODEL, PROVIDER, DEEPSEEK_MODEL, DEEPSEEK_HOST, DEEPSEEK_KEY
from providers.AbstractProvider import ChatProvider


def create_provider() -> ChatProvider:
    """
    Instantiate the configured ChatProvider.
    Add new providers here as they are implemented.
    """
    if PROVIDER == "ollama":
        from providers.OllamaProvider import OllamaProvider
        return OllamaProvider(model=OLLAMA_MODEL, host=OLLAMA_HOST)
    if PROVIDER == "deepseek":
        from providers.DeepseekProvider import DeepseekProvider
        return DeepseekProvider(model=DEEPSEEK_MODEL, host=DEEPSEEK_HOST, key=DEEPSEEK_KEY)

    raise ValueError(f"Unknown provider '{PROVIDER}'. Set the PROVIDER env var to a supported value.")
