import { Link } from 'react-router-dom';

export default function MenuLinks() {
    return (
        <div className="flex flex-col items-center gap-[10px]">
            <Link to="/form" className="min-w-[200px]">
                <button className="w-full">Form</button>
            </Link>
            <Link to="/jobs" className="min-w-[200px]">
                <button className="w-full">View Jobs</button>
            </Link>
            <Link to="devices" className="min-w-[200px]">
                <button className="w-full">Manage Devices</button>
            </Link>
        </div>
    )
}