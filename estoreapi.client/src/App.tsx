import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom';
import HomePage from './pages/home/HomePage';
import FormPage from './pages/form/FormPage';
import JobsPage from './pages/jobs/JobsPage';
import DevicesPage from './pages/devices/DevicesPage';
import Chat from './components/Chat';
import { Navbar } from './components/Navbar';
import { TooltipProvider } from './components/ui/tooltip';
import { ToastContainer } from 'react-toastify';

const pageTitles: Record<string, string> = {
    '/': '',
    '/form': 'E-Store Repair Job Form',
    '/jobs': 'Jobs',
    '/devices': 'Devices',
};

function AppContent() {
    const { pathname } = useLocation();
    const title = pageTitles[pathname];

    return (
        <Chat>
            <Navbar title={title}>
                <Routes>
                    <Route path="/" element={<HomePage />} />
                    <Route path="/form" element={<FormPage title={title} />} />
                    <Route path="/jobs" element={<JobsPage title={title} />} />
                    <Route path="/devices" element={<DevicesPage title={title} />} />
                </Routes>
            </Navbar>
        </Chat>
    );
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