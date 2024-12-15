import './App.css';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import MenuPage from './components/MenuPage';
import FormPage from './components/FormPage';
import JobsPage from './components/JobsPage';
import DevicesPage from './components/DevicesPage';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<MenuPage />} />
                <Route path="/form" element={<FormPage />} />
                <Route path="/jobs" element={<JobsPage />} />
                <Route path="/devices" element={<DevicesPage />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;