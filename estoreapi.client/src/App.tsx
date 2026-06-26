import { BrowserRouter, Routes, Route, useLocation, Outlet } from 'react-router-dom';
import HomePage from './pages/home/HomePage';
import FormPage from './pages/form/FormPage';
import JobsPage from './pages/jobs/JobsPage';
import DevicesPage from './pages/devices/DevicesPage';
import NotFoundPage from './pages/404/NotFoundPage';
import Chat from './components/chat/Chat';
import { Navbar } from './components/Navbar';
import { TooltipProvider } from './components/ui/tooltip';
import { ToastContainer } from 'react-toastify';
import LoginPage from './pages/login/LoginPage';
import RouteGuard from './components/RouteGuard';
import SettingsPage from './pages/settings/SettingsPage';

const pageTitles: Record<string, string> = {
    '/': '',
    '/form': 'E-Store Repair Job Form',
    '/jobs': 'Jobs',
    '/devices': 'Devices',
    '/settings': 'Settings',
};

function AppLayout() {
    const { pathname } = useLocation();
    const title = pageTitles[pathname];

    return (
        <Chat>
            <Navbar title={title}>
                <Outlet />
            </Navbar>
        </Chat>
    );
}

function AppContent() {
    const { pathname } = useLocation();
    const title = pageTitles[pathname];

    return (
        <Routes>
            {/** login page only */}
            <Route path="/login" element={<LoginPage />} />

            {/** regular app with navbar, chat panel */}
            <Route element={<RouteGuard />}>
                <Route element={<AppLayout />}>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/form" element={<FormPage title={title} />} />
                    <Route path="/jobs" element={<JobsPage title={title} />} />
                    <Route path="/devices" element={<DevicesPage title={title} />} />
                    <Route path="/settings" element={<SettingsPage title={title} />} />
                    <Route path="*" element={<NotFoundPage />} />
                </Route>
            </Route>
        </Routes>
    )
}

export default function App() {
    return (
        <BrowserRouter>
            <TooltipProvider>
                <AppContent />
            </TooltipProvider>
            <ToastContainer />
        </BrowserRouter>
    );
}