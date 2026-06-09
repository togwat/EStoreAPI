"""
Per-request user identity carried via a ContextVar so tool handlers (e.g. memory_search)
can read the current user_id without it being threaded through every call parameter.

Set once per request in chat.py before the agentic loop runs:
    user_id_var.set(req.user_id)

When ASP.NET auth is added, the only change needed is where the value is sourced from
before calling set() — everything downstream stays identical.
"""
from contextvars import ContextVar

user_id_var: ContextVar[str] = ContextVar("user_id", default="default")
