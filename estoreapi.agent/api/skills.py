"""
Agent skill files CRUD endpoints (`/agent/skills`)

The agent uses SkillProvider directly. These endpoints are for human-interactable frontend,
namely the skills page.
"""
from fastapi import APIRouter, Depends, HTTPException, Response
from pydantic import BaseModel

from skills.SkillProvider import SkillProvider
from dependencies import get_skills, get_user_email

# depend on user email for auth
router = APIRouter(prefix="/agent/skills", tags=["skills"], dependencies=[Depends(get_user_email)])


class SkillCreateRequest(BaseModel):
    name: str
    summary: str
    content: str


class SkillUpdateRequest(BaseModel):
    summary: str | None = None
    content: str | None = None


@router.get("")
def list_skills(provider: SkillProvider = Depends(get_skills)):
    """List every saved skill as {name, summary}"""
    return provider.list_skills()

@router.get("/{name}")
def get_skill(
    name: str,
    provider: SkillProvider = Depends(get_skills),
):
    """Return a skill in the format {name, summary, content}"""
    skill = provider.get_skill(name)
    if skill is None:
        raise HTTPException(status_code=404, detail="Skill not found")
    return skill

@router.post("", status_code=201)
def create_skill(
    req: SkillCreateRequest,
    provider: SkillProvider = Depends(get_skills),
):
    """Create a skill. The name is the unique id, so a clash is a conflict, not an overwrite."""
    if not provider.create_skill(req.name, req.summary, req.content):
        raise HTTPException(status_code=409, detail=f"Skill '{req.name}' already exists")
    return Response(status_code=201)

@router.patch("/{name}")
def update_skill(
    name: str,
    req: SkillUpdateRequest,
    provider: SkillProvider = Depends(get_skills),
):
    """
    Update a skill's summary or content. 
    If either are empty/none, the fields stay as-is.
    """
    if not req.summary and not req.content:
        raise HTTPException(status_code=400, detail="Provide summary and/or content")
    if not provider.update_skill(name, req.summary, req.content):
        raise HTTPException(status_code=404, detail="Skill not found")
    return Response(status_code=204)

@router.delete("/{name}")
def delete_skill(
    name: str,
    provider: SkillProvider = Depends(get_skills),
):
    """Deletes the skill with the given name."""
    if not provider.delete_skill(name):
        raise HTTPException(status_code=404, detail="Skill not found")
    return Response(status_code=204)
