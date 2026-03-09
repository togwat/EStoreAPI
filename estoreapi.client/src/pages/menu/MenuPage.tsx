import { Link } from 'react-router-dom';

function MenuPage() {
    return (
        <div>
            <Link to="/form">
                <button>Form</button>
            </Link>
            <Link to="/jobs">
                <button>View Jobs</button>
            </Link>
            <Link to="devices">
                <button>Manage Devices</button>
            </Link>
        </div>
    );
}

export default MenuPage;