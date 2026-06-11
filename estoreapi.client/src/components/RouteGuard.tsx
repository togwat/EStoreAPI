import { useEffect, useState } from 'react';
import { Navigate, Outlet } from 'react-router-dom';
import { getMe } from '@/api/auth';

// confirms a session exists before rendering any child route
// prevents flashes of the webpage before being authenticated
// frontend only
export default function RouteGuard() {
    const [status, setStatus] = useState<'loading' | 'allowed' | 'denied'>('loading');

    useEffect(() => {
        // call getMe to check auth status
        getMe().then((profile) => setStatus(profile ? 'allowed' : 'denied'));
    }, []);

    // brief blank screen while the session check runs
    if (status === 'loading') {
        return null;
    }

    // unauthenticated, go to login page
    if (status === 'denied') {
        return <Navigate to="/login" replace />;
    }

    return <Outlet />;
}