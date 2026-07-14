from datetime import datetime, timezone


def get_time() -> str:
    """Returns the current UTC and local time as an ISO 8601 string."""
    utc = datetime.now(timezone.utc).isoformat()
    local = datetime.now().isoformat()
    response = f"UTC: {utc}\nLocal time: {local}"
    return response
