from tools.AbstractToolClient import AbstractToolClient


class ToolRouter:
    """
    Maps tool names to the AbstractToolClient that owns them.
    """

    def __init__(self, clients: list[AbstractToolClient]):
        self._clients = clients
        self._route_map: dict[str, AbstractToolClient] | None = None

    def route(self, tool_name: str) -> AbstractToolClient:
        """Return the client responsible for the given tool name."""
        return self._get_route_map()[tool_name]

    def _get_route_map(self) -> dict[str, AbstractToolClient]:
        """
        The route map is built lazily on the first route() call, not at construction
        time. This avoids calling list_tools() inside the async lifespan handler,
        where McpClient's asyncio.run() would fail due to a running event loop.
        """
        if self._route_map is None:
            self._route_map = {}
            for client in self._clients:
                for tool in client.list_tools():
                    # Earlier clients in the list take priority on name conflicts.
                    if tool["name"] not in self._route_map:
                        self._route_map[tool["name"]] = client
        return self._route_map
