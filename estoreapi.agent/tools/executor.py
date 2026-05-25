import requests


class Executor:
    """Dispatches tool calls to the ASP.NET API over HTTP."""

    def __init__(self, base_url: str):
        self._base = base_url.rstrip("/")

    def dispatch(self, tool_name: str, inputs: dict) -> str:
        """Execute a named tool and return the API response as a string."""
        handler = self._routes.get(tool_name)
        if handler is None:
            return f"Unknown tool: {tool_name}"
        try:
            method, url, body = handler(self._base, inputs)
            response = requests.request(method, url, json=body, timeout=10)
            response.raise_for_status()
            return response.text or "OK"
        except requests.HTTPError as e:
            return f"HTTP error {e.response.status_code}: {e.response.text}"
        except Exception as e:
            return f"Error: {e}"

    @staticmethod
    def _body_without_id(inputs: dict) -> dict:
        """Strip the 'id' path param from a PUT request body."""
        return {k: v for k, v in inputs.items() if k != "id"}

    # Route table
    # Each entry maps a tool name -> (method, url, body) given (base_url, inputs)
    _routes = {
        # Customers
        "get_all_customers":      lambda b, _: ("GET",  f"{b}/api/Customers",              None),
        "get_customer_by_id":     lambda b, i: ("GET",  f"{b}/api/Customers/{i['id']}",    None),
        "search_customers":       lambda b, i: ("GET",  f"{b}/api/Customers/search?query={i['query']}", None),
        "create_customer":        lambda b, i: ("POST", f"{b}/api/Customers/create",        i),
        "create_customers_bulk":  lambda b, i: ("POST", f"{b}/api/Customers/create-bulk",   i["customers"]),
        "update_customer":        lambda b, i: ("PUT",  f"{b}/api/Customers/update/{i['id']}", Executor._body_without_id(i)),

        # Devices
        "get_all_devices":        lambda b, _: ("GET",  f"{b}/api/Devices",                None),
        "get_device_by_id":       lambda b, i: ("GET",  f"{b}/api/Devices/{i['id']}",      None),
        "search_devices_by_name": lambda b, i: ("GET",  f"{b}/api/Devices/searchName?name={i['name']}", None),
        "search_devices_by_type": lambda b, i: ("GET",  f"{b}/api/Devices/searchType?type={i['type']}", None),
        "create_device":          lambda b, i: ("POST", f"{b}/api/Devices/create",          i),
        "create_devices_bulk":    lambda b, i: ("POST", f"{b}/api/Devices/create-bulk",     i["devices"]),
        "update_device":          lambda b, i: ("PUT",  f"{b}/api/Devices/update/{i['id']}", Executor._body_without_id(i)),

        # Jobs
        "get_all_jobs":           lambda b, _: ("GET",  f"{b}/api/Jobs",                   None),
        "get_job_by_id":          lambda b, i: ("GET",  f"{b}/api/Jobs/{i['id']}",         None),
        "create_job":             lambda b, i: ("POST", f"{b}/api/Jobs/create",             i),
        "create_jobs_bulk":       lambda b, i: ("POST", f"{b}/api/Jobs/create-bulk",        i["jobs"]),
        "update_job":             lambda b, i: ("PUT",  f"{b}/api/Jobs/update/{i['id']}",   Executor._body_without_id(i)),

        # Problems
        "get_problem_by_id":      lambda b, i: ("GET",  f"{b}/api/Problems/{i['id']}",     None),
        "get_problems_by_device": lambda b, i: ("GET",  f"{b}/api/Problems?deviceId={i['deviceId']}", None),
        "create_problem":         lambda b, i: ("POST", f"{b}/api/Problems/create",         i),
        "create_problems_bulk":   lambda b, i: ("POST", f"{b}/api/Problems/create-bulk",    i["problems"]),
        "update_problem":         lambda b, i: ("PUT",  f"{b}/api/Problems/update/{i['id']}", Executor._body_without_id(i)),
    }
