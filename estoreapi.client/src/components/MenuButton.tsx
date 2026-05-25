import { Link } from 'react-router-dom';

function MenuButton() {
    return (
        <Link to="/" className="absolute top-0 left-0 mt-16 mx-32 w-32">
            <button className="w-full">Back</button>
        </Link>
    );
}

export default MenuButton;