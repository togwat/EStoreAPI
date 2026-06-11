import { api } from './client';

export type Profile = {
    email: string
    name: string
}

function _mapProfile(p: { email: string, name: string }): Profile {
    return {
        email: p.email,
        name: p.name
    }
}

// current session's profile, or null when signed out
// not used yet, good for a profile page later on
export async function getMe(): Promise<Profile | null> {
    try {
        const response = await api.get("api/auth/me");
        return _mapProfile(response.data);
    } catch {
        // 401 on /login is expected (no session yet) and must not throw;
        // everywhere else the client interceptor already redirected
        return null;
    }
}

export async function logout(): Promise<void> {
    await api.post("api/auth/logout");
}
