from tavily import TavilyClient
from config import TAVILY_KEY

_client = TavilyClient(api_key=TAVILY_KEY)

def _format_result(response: dict) -> str:
    """Makes result compatible with assistant-ui's sources component"""
    import json
    sources = [
        {"url": r["url"], "title": r.get("title", "")}
        for r in response["results"]
    ]
    text = "\n\n".join(
        f"{r.get('title', '')}\n{r['url']}\n{r.get('content', '')}"
        for r in response["results"]
    )
    return json.dumps({"text": text, "sources": sources})


def web_search(query: str) -> str:
    response = _client.search(query, 
        max_results=5, 
        search_depth="basic",   # basic costs 1 credit, advanced 2
        country="new zealand"
    )

    return _format_result(response)
