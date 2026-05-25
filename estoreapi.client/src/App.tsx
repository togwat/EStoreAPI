import { BrowserRouter, Routes, Route } from 'react-router-dom';
import HomePage from './pages/home/HomePage';
import FormPage from './pages/form/FormPage';
import JobsPage from './pages/jobs/JobsPage';
import DevicesPage from './pages/devices/DevicesPage';
import Chat from './components/Chat';
import { Navbar } from './components/Navbar';

function App() {
    return (
        <BrowserRouter>
            <Chat>
                <Navbar>
                    <Routes>
                        <Route path="/" element={<HomePage />} />
                        <Route path="/form" element={<FormPage />} />
                        <Route path="/jobs" element={<JobsPage />} />
                        <Route path="/devices" element={<DevicesPage />} />
                    </Routes>
                </Navbar>
            </Chat>
        </BrowserRouter>
    );
}

export default App;