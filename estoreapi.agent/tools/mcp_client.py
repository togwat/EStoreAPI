import asyncio

from mcp import ClientSession
from mcp.client.streamable_http import streamable_http_client
from mcp.types import TextContent


class McpClient:
    """Sync wrapper around the async MCP SDK for use in sync FastAPI endpoints."""

    def __init__(self, url: str):
        self.url = url

    # public sync API
    def list_tools(self) -> list[dict]:
        """Return tools in {name, description, input_schema} format expected by providers."""
        return asyncio.run(self._list_tools_async())

    def call_tool(self, name: str, arguments: dict) -> str:
        """Call a named MCP tool and return the result as a string."""
        return asyncio.run(self._call_tool_async(name, arguments))

    # async internals 
    async def _list_tools_async(self) -> list[dict]:
        async with streamable_http_client(self.url) as (read, write, _):
            async with ClientSession(read, write) as session:
                await session.initialize()
                result = await session.list_tools()
                return [
                    {
                        "name": t.name,
                        "description": t.description or "",
                        "input_schema": t.inputSchema,
                    }
                    for t in result.tools
                ]

    async def _call_tool_async(self, name: str, arguments: dict) -> str:
        async with streamable_http_client(self.url) as (read, write, _):
            async with ClientSession(read, write) as session:
                await session.initialize()
                result = await session.call_tool(name, arguments)
                parts = [item.text for item in result.content if isinstance(item, TextContent)]
                return "\n".join(parts) if parts else "OK"
