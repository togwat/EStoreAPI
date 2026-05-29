from tools.mcp.McpClient import McpClient
from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService


class CorrectingMcpClient(McpClient):
    def __init__(self, url: str, desc_service: AbstractDescriptionService):
        super().__init__(url)
        self._service = desc_service

    async def _list_tools_async(self) -> list[dict]:
        tools = await super()._list_tools_async()
        return [self._apply_overrides(t) for t in tools]

    def _apply_overrides(self, tool: dict) -> dict:
        result = dict(tool)

        tool_desc = self._service.get(tool["name"])
        if tool_desc:
            result["description"] = tool_desc

        schema = dict(tool.get("input_schema", {}))
        props = schema.get("properties", {})
        if props:
            updated = {}
            for param, param_schema in props.items():
                override = self._service.get(f"{tool['name']}.{param}")
                if override:
                    updated[param] = {**param_schema, "description": override}
                else:
                    updated[param] = param_schema
            result["input_schema"] = {**schema, "properties": updated}

        return result
