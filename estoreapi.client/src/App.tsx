import { BrowserRouter, Routes, Route } from 'react-router-dom';
import MenuPage from './pages/menu/MenuPage';
import FormPage from './pages/form/FormPage';
import JobsPage from './pages/jobs/JobsPage';
import DevicesPage from './pages/devices/DevicesPage';
import Chat from './components/Chat';

function App() {
    return (
        <BrowserRouter>
            <div className="max-w-7xl mx-auto my-8 p-8 text-center">
                <Routes>
                    <Route path="/" element={<MenuPage />} />
                    <Route path="/form" element={<FormPage />} />
                    <Route path="/jobs" element={<JobsPage />} />
                    <Route path="/devices" element={<DevicesPage />} />
                </Routes>
            </div>
            <Chat />
        </BrowserRouter>
    );
}

export default App;