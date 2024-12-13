import './App.css';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import MenuPage from './components/MenuPage';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<MenuPage />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;