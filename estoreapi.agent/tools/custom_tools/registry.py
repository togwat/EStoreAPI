from dataclasses import dataclass

from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService


@dataclass
class CustomTool:
    """
    schema               — tool definition in {name, description, input_schema} format
                           expected by providers. The description field is a placeholder;
                           Registry.get_schemas() overlays the live value from the
                           description service at call time.
    default_description  — fallback used when the description service has no override
                           for this tool's name.
    """
    schema: dict
    default_description: str


class Registry:
    """
    Owns the definitions (schemas + default descriptions) for all custom tools.
    """

    def __init__(self, desc_service: AbstractDescriptionService):
        self._service = desc_service
        self._tools: dict[str, CustomTool] = {}
        self._register_get_time()
        self._register_update_description()
        self._register_web_search()
        self._register_web_fetch()
        self._register_memory_search()

    def __contains__(self, name: str) -> bool:
        return name in self._tools

    def get_schemas(self) -> list[dict]:
        """Return tool definitions with descriptions resolved from the service at call time."""
        result = []
        for name, tool in self._tools.items():
            schema = dict(tool.schema)
            # Always fetch the latest description; fall back to the registry default.
            schema["description"] = self._service.get(name) or tool.default_description
            result.append(schema)
        return result

    # --- tool registrations ---

    def _register_get_time(self):
        default = "Returns the current UTC time as an ISO 8601 string."
        self._tools["get_time"] = CustomTool(
            schema={
                "name": "get_time",
                "description": default,
                "input_schema": {"type": "object", "properties": {}, "required": []},
            },
            default_description=default,
        )

    def _register_update_description(self):
        default = (
            "Overrides the description for a tool or one of its parameters."
            "Before calling this tool, verify the exact tool name and parameter name from the tool list, do not guess or infer names."
            "Use tool_name alone to update the tool description. Add parameter_name to update a specific parameter's description instead."
        )
        self._tools["update_description"] = CustomTool(
            schema={
                "name": "update_description",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "tool_name": {
                            "type": "string",
                            "description": "Exact name of the tool to update, as it appears in the tool list.",
                        },
                        "description": {"type": "string", "description": "The new description."},
                        "parameter_name": {
                            "type": "string",
                            "description": (
                                "Exact name of the parameter to update, as it appears in the tool's input schema. "
                                "If omitted, the tool-level description is updated instead."
                            ),
                        },
                    },
                    "required": ["tool_name", "description"],
                },
            },
            default_description=default,
        )

    def _register_web_search(self):
        default = "Search the web for current information (recent events, prices, up-to-date data). Returns the top 5 results, each with a short summary. Use web_fetch on a result's URL to read a full page."
        self._tools["web_search"] = CustomTool(
            schema={
                "name": "web_search",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "query": {
                            "type": "string",
                            "description": "The search query.",
                        }
                    },
                    "required": ["query"],
                },
            },
            default_description=default,
        )

    def _register_web_fetch(self):
        default = (
            "Fetch a specific web page and return its full contents as readable text."
            "Use when the user gives you an exact URL to read, or to explore a promising web_search result in depth."
        )
        self._tools["web_fetch"] = CustomTool(
            schema={
                "name": "web_fetch",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "url": {
                            "type": "string",
                            "description": "The full URL of the page to fetch, including https://",
                        }
                    },
                    "required": ["url"],
                },
            },
            default_description=default,
        )

    def _register_memory_search(self):
        default = "Search your persistent memory for facts relevant to the query. Use to recall information from past conversations."
        self._tools["memory_search"] = CustomTool(
            schema={
                "name": "memory_search",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "query": {
                            "type": "string",
                            "description": "What to recall from memory.",
                        }
                    },
                    "required": ["query"],
                },
            },
            default_description=default,
        )