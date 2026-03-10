import axios from 'axios';
import '../Form.css';

export default function JobForm() {
    async function handleSubmit(event: React.ChangeEvent<HTMLFormElement>) {
        event.preventDefault();

        // retrieve all form data
        const formData = new FormData(event.currentTarget);
        
        const name = formData.get("name")?.toString().trim();
        const phone = formData.get("phone")!.toString().trim();
        const phone2 = formData.get("phone2")?.toString().trim();
        const email = formData.get("email")?.toString().trim();
        const address = formData.get("address")?.toString().trim();
                
        // add/assign customer
        // check if an existing customer matches (using primary phone number)
        await axios.get('/api/Customers/search', {
            withCredentials: false,
            params: {
                query: phone
            }
        }).then((response) => {
            alert(response.data);
        }).catch((error) => {
            alert(error);
        });
        // TODO: add new job
        console.log(`${name} ${phone} ${phone2} ${email} ${address}`);
    }

    return (
        <form className="job-form" onSubmit={handleSubmit} >
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