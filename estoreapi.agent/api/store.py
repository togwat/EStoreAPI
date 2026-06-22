"""
Chat history persistence endpoints (`/agent/store`).

These back assistant-ui's RemoteThreadListAdapter (thread list) and ThreadHistoryAdapter
(per-thread messages) on the frontend. Distinct from `/agent/chat`, which runs the agent;
this router only reads/writes saved sessions. Every route is scoped to the calling user
via `get_user_email`, and ownership is enforced in the store (404 on mismatch).
"""
from fastapi import APIRouter, Depends, HTTPException, Response

from pydantic import BaseModel

from dependencies import get_store, get_user_email
from store.AbstractChatStore import AbstractChatStore

router = APIRouter(prefix="/agent/store", tags=["store"])


class InitializeRequest(BaseModel):
    threadId: str


class PatchRequest(BaseModel):
    title: str | None = None


class AppendRequest(BaseModel):
    message: dict
    parentId: str | None = None


@router.get("")
def list_threads(
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """Adapter `list`: all of the user's sessions, newest first."""
    return {"threads": store.list_sessions(user_email)}


@router.post("")
def initialize_thread(
    req: InitializeRequest,
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """Adapter `initialize`: create (or no-op return) the client-provided thread id."""
    remote_id = store.create_session(user_email, req.threadId)
    return {"remoteId": remote_id, "externalId": None}


@router.get("/{session_id}")
def fetch_thread(
    session_id: str,
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """Adapter `fetch`: a single session's metadata."""
    session = store.get_session(user_email, session_id)
    if session is None:
        raise HTTPException(status_code=404, detail="Session not found")
    return {"remoteId": session["remoteId"], "title": session["title"]}


@router.patch("/{session_id}")
def patch_thread(
    session_id: str,
    req: PatchRequest,
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """Adapter `rename`: update the session title."""
    if req.title is not None and not store.rename_session(user_email, session_id, req.title):
        raise HTTPException(status_code=404, detail="Session not found")
    return Response(status_code=204)


@router.delete("/{session_id}")
def delete_thread(
    session_id: str,
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """Adapter `delete`: remove the session and its messages."""
    if not store.delete_session(user_email, session_id):
        raise HTTPException(status_code=404, detail="Session not found")
    return Response(status_code=204)


@router.get("/{session_id}/messages")
def get_messages(
    session_id: str,
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """History `load`: the session's full message repository for restore."""
    repo = store.get_messages(user_email, session_id)
    if repo is None:
        raise HTTPException(status_code=404, detail="Session not found")
    return repo


@router.post("/{session_id}/messages")
def append_message(
    session_id: str,
    req: AppendRequest,
    user_email: str = Depends(get_user_email),
    store: AbstractChatStore = Depends(get_store),
):
    """History `append`: persist one finalized message ({message, parentId})."""
    if not store.append_message(user_email, session_id, req.model_dump()):
        raise HTTPException(status_code=404, detail="Session not found")
    return Response(status_code=204)