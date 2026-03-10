import MenuLinks from './components/MenuLinks';
import MenuHeader from './components/MenuHeader';
import './Menu.css';


export default function MenuPage() {
    return (
        <div className="menu-container">
            <MenuHeader />
            <MenuLinks />
        </div>
    );
}
