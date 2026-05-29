from tools.descriptions.AbstractDescriptionService import AbstractDescriptionService


def make_update_description_handler(desc_service: AbstractDescriptionService):
    """
    Returns a handler for the update_description tool bound to the given service.

    Key format:
      "tool_name"            — updates the tool-level description.
      "tool_name.param_name" — updates a specific parameter's description.
    CorrectingMcpClient uses the same convention when applying parameter overrides.
    """
    def update_description(tool_name: str, description: str, parameter_name: str | None = None) -> str:
        key = f"{tool_name}.{parameter_name}" if parameter_name else tool_name
        return desc_service.update(key, description)

    return update_description
