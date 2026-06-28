"""
Single source of truth for tools that require explicit user confirmation before
they execute.

The agentic loop (see api/chat.py) checks every tool the model wants to call
against this registry. Gated tools are NOT run server-side; instead the loop
pauses and hands control to the frontend, which shows a Confirm/Cancel UI and
only runs the tool (via POST /agent/tool) once the user approves.
"""

# Tool names that must be confirmed by the user before running.
# Take MCP method name in C#, make it snake_case and remove Async suffix
CONFIRMATION_REQUIRED_TOOLS: set[str] = {
    "create_customers",
    "create_devices",
    "create_problems",
    "create_jobs",
    "update_description",
    "update_customers",
    "update_devices",
    "update_problems",
    "update_jobs",
    "submit_form",
    "web_search",
    "web_fetch"
}


def requires_confirmation(tool_name: str) -> bool:
    """Return True if calling `tool_name` requires explicit user confirmation."""
    return tool_name in CONFIRMATION_REQUIRED_TOOLS
