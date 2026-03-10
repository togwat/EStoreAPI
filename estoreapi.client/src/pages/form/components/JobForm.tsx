import axios from 'axios';
import '../Form.css';

export default function JobForm() {
    async function addJob(event: React.FormEvent) {
        event.preventDefault();
        // add/assign customer
        const formData = event.target as HTMLFormElement;

        const name: string = formData.name.value;   // ignore ts warning, this works
        const phone: string = formData.phone.value;
        const phone2: string = formData.phone2.value;
        const email: string = formData.email.value;
        const address: string = formData.address.value;

        // check if an existing customer matches (using primary phone number)
        await axios.get('https://localhost:7211/api/Customers/search', {
            withCredentials: false,
            params: {
                query: phone
            }
        }).then((response) => {
            alert(response.data);
        }).catch((error) => {
            alert(error);
        });
        // add new job
    }

    return (
        <form className="job-form" onSubmit={addJob} >
            <div className="job-form-field">
                <label htmlFor="name">Name</label>
                <input name="name" />
            </div>
            <div className="job-form-field">
                <label htmlFor="phone" className="required">Phone number</label>
                <input name="phone" placeholder="required" />
            </div>
            <div className="job-form-field">
                <label htmlFor="phone2">Secondary phone number</label>
                <input name="phone2" />
            </div>
            <div className="job-form-field">
                <label htmlFor="email">Email</label>
                <input name="email" />
            </div>
            <div className="job-form-field">
                <label htmlFor="address">Address</label>
                <input name="address" />
            </div>
            <div className="job-form-field">
                <label htmlFor="Device">Device</label>
                <input name="device" />
            </div>
            <div className="job-form-field">
                <label htmlFor="notes">Notes</label>
                <textarea name="notes" />
            </div>
            <div className="job-form-field">
                <label htmlFor="price">Estimated price</label>
                <input name="price" />
            </div>
            <button type="submit">Submit</button>
        </form>
    )
}