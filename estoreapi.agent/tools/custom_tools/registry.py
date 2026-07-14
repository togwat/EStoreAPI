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

    def __init__(self, desc_service: AbstractDescriptionService, memory_enabled: bool = False):
        self._service = desc_service
        self._tools: dict[str, CustomTool] = {}
        self._register_get_time()
        self._register_update_description()
        self._register_web_search()
        self._register_web_fetch()
        if memory_enabled:
            self._register_memory_search()
        self._register_get_skill()
        self._register_create_skill()
        self._register_update_skill()
        self._register_delete_skill()

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

    def _register_get_skill(self):
        default = (
            "Retrieve the full markdown document for a saved skill. "
            "Call this before starting a task that matches a skill in the skill index, then follow the document's instructions."
        )
        self._tools["get_skill"] = CustomTool(
            schema={
                "name": "get_skill",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "Exact name of the skill, as it appears in the skill index.",
                        }
                    },
                    "required": ["name"],
                },
            },
            default_description=default,
        )

    def _register_create_skill(self):
        default = (
            "Save a reusable skill document so the procedure can be repeated in future sessions. "
            "Use after completing a multi-step procedure and the user asks you to remember it. "
            "Write for a future session that has no context of this conversation; never include session-specific data such as customer names, IDs, or dates."
        )
        self._tools["create_skill"] = CustomTool(
            schema={
                "name": "create_skill",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "Short unique snake case title identifying the skill, e.g. 'intake_new_repair_job'.",
                        },
                        "summary": {
                            "type": "string",
                            "description": "One sentence stating what the skill does and when to use it. Shown in the skill index of every session.",
                        },
                        "content": {
                            "type": "string",
                            "description": (
                                "The full skill document in markdown: goal, preconditions, numbered steps naming the exact tools to call, and known pitfalls."
                            ),
                        },
                    },
                    "required": ["name", "summary", "content"],
                },
            },
            default_description=default,
        )

    def _register_update_skill(self):
        default = (
            "Update a saved skill's summary and/or content, e.g. when following it revealed the procedure is wrong or outdated. "
            "Provide only the fields to change; omitted fields stay as-is."
        )
        self._tools["update_skill"] = CustomTool(
            schema={
                "name": "update_skill",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "Exact name of the skill to update, as it appears in the skill index.",
                        },
                        "summary": {
                            "type": "string",
                            "description": "The replacement one-sentence summary. Omit to keep the current one.",
                        },
                        "content": {
                            "type": "string",
                            "description": "The replacement markdown document. Omit to keep the current one.",
                        },
                    },
                    "required": ["name"],
                },
            },
            default_description=default,
        )

    def _register_delete_skill(self):
        default = (
            "Permanently delete a saved skill that is obsolete or no longer applies to the system. "
            "Prefer update_skill when the procedure can be corrected instead."
        )
        self._tools["delete_skill"] = CustomTool(
            schema={
                "name": "delete_skill",
                "description": default,
                "input_schema": {
                    "type": "object",
                    "properties": {
                        "name": {
                            "type": "string",
                            "description": "Exact name of the skill to delete, as it appears in the skill index.",
                        }
                    },
                    "required": ["name"],
                },
            },
            default_description=default,
        )