from dataclasses import dataclass

from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService


@dataclass
class CustomTool:
    """
    Pure definition of a custom (non-MCP) tool.

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
            "Overrides the description for any tool or tool parameter. "
            "Use tool_name alone to update the tool description, or add parameter_name to target a specific parameter."
        )
        self._tools["update_description"] = CustomTool(
            schema={
                "name": "update_description",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "tool_name": {"type": "string", "description": "The tool to update."},
                        "description": {"type": "string", "description": "The new description."},
                        "parameter_name": {
                            "type": "string",
                            "description": "If provided, updates that parameter's description instead of the tool's.",
                        },
                    },
                    "required": ["tool_name", "description"],
                },
            },
            default_description=default,
        )