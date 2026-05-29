from collections.abc import Callable

from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService


def make_update_description_handler(
    desc_service: AbstractDescriptionService,
    get_all_tools: Callable[[], list[dict]] | None = None,
):
    """
    Returns a handler for the update_description tool bound to the given service.

    get_all_tools — optional lazy callable returning all tool schemas. Called at
                    invocation time (not construction time) to avoid circular
                    dependencies. When provided, rejects unknown tool names and
                    parameter names before writing.

    Key format:
      "tool_name"            — updates the tool-level description.
      "tool_name.param_name" — updates a specific parameter's description.
    CorrectingMcpClient uses the same convention when applying parameter overrides.
    """
    def update_description(tool_name: str, description: str, parameter_name: str | None = None) -> str:
        # verify tool existence
        if get_all_tools is not None:
            tools_by_name = {t["name"]: t for t in get_all_tools()}
            if tool_name not in tools_by_name:
                return f"Unknown tool '{tool_name}'. No description updated."
            if parameter_name is not None:
                props = tools_by_name[tool_name].get("input_schema", {}).get("properties", {})
                if parameter_name not in props:
                    return f"Tool '{tool_name}' has no parameter '{parameter_name}'. No description updated."
        
        return desc_service.update(tool_name, description, parameter_name)

    return update_description
