import { Link } from 'react-router-dom';

export default function MenuLinks() {
    return (
        <div className="links-container">
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
    )
}