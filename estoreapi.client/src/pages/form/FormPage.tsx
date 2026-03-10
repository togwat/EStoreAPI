import MenuButton from '../../components/MenuButton';
import JobForm from './components/JobForm';
import './Form.css';

export default function FormPage() {
    return (
        <div>
            <MenuButton />
            <h1>E-Store Repair Job Form</h1>
            <JobForm />
        </div>
    );
}
