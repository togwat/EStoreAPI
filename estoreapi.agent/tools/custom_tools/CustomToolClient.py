from collections.abc import Callable

from tools.AbstractToolClient import AbstractToolClient
from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService
from tools.custom_tools.registry import Registry
from tools.custom_tools.memory_search import make_memory_search_handler
from tools.custom_tools.time_lookup import get_time
from tools.custom_tools.update_description import make_update_description_handler
from tools.custom_tools.web_search import web_search
from tools.custom_tools.web_search import web_fetch


class CustomToolClient(AbstractToolClient):
    """
    Execution layer for custom (non-MCP) tools.

    list_tools() delegates to the Registry for schema/description resolution.
    call_tool()  dispatches to individual tool functions by name.
    """

    def __init__(
        self,
        registry: Registry,
        desc_service: AbstractDescriptionService,
        memory=None,
        get_all_tools: Callable[[], list[dict]] | None = None,
    ):
        self._registry = registry
        # Map tool names to their callable handlers.
        self._handlers = {
            "get_time": get_time,
            "update_description": make_update_description_handler(desc_service, get_all_tools),
            "web_search": web_search,
            "web_fetch": web_fetch,
            "memory_search": make_memory_search_handler(memory),
        }

    def __contains__(self, name: str) -> bool:
        return name in self._registry

    def list_tools(self) -> list[dict]:
        """Return live tool definitions sourced from the Registry."""
        return self._registry.get_schemas()

    def call_tool(self, name: str, arguments: dict) -> str:
        """Dispatch to the handler for the named tool."""
        return self._handlers[name](**arguments)
