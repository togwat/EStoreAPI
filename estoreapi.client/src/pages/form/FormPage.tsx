import MenuButton from '../../components/MenuButton';
import JobForm from './components/JobForm';

export default function FormPage() {
    return (
        <div>
            <MenuButton />
            <h1 className="mb-8">E-Store Repair Job Form</h1>
            <JobForm />
        </div>
    );
}
