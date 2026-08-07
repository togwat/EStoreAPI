/**
 * CRUD for agent skills MD files
 */
import { toast } from '@/components/CustomToast';
import { api } from './client';
import { handleApiError } from './apiHelpers';

// listSkills only return names and summaries
export type SkillSummary = {
    name: string
    summary: string
}

// Only load content when a skill is specified
export type Skill = SkillSummary & {
    content: string
}

export type SkillUpdate = {
    summary?: string
    content?: string
}

export async function listSkills(): Promise<SkillSummary[]> {
    try {
        const response = await api.get<SkillSummary[]>('/agent/skills');
        return response.data;
    } catch (error) {
        handleApiError(error, {}, "Couldn't load skills");
    }
}

export async function getSkill(name: string): Promise<Skill> {
    try {
        const response = await api.get<Skill>(`/agent/skills/${encodeURIComponent(name)}`);
        return response.data;
    } catch (error) {
        handleApiError(error, {
            404: "Skill not found."
        }, "Couldn't load skill");
    }
}

export async function createSkill(skill: Skill): Promise<void> {
    try {
        await api.post('/agent/skills', skill);
        toast.success("Skill created", skill.name);
    } catch (error) {
        handleApiError(error, {
            409: "A skill with that name already exists.",
            422: "One or more validation errors occurred.",
        }, "Couldn't create skill");
    }
}

export async function updateSkill(name: string, updates: SkillUpdate): Promise<void> {
    try {
        await api.patch(`/agent/skills/${encodeURIComponent(name)}`, updates);
        toast.success("Skill saved", name);
    } catch (error) {
        handleApiError(error, {
            404: "Skill not found.",
            400: "Provide a summary or content to update.",
        }, "Couldn't save skill");
    }
}

export async function deleteSkill(name: string): Promise<void> {
    try {
        await api.delete(`/agent/skills/${encodeURIComponent(name)}`);
        toast.success("Skill deleted", name);
    } catch (error) {
        handleApiError(error, {
            404: "Skill not found."
        }, "Couldn't delete skill");
    }
}
