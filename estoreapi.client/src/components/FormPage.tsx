import MenuButton from './MenuButton';
import axios from 'axios';

function FormPage() {
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
        <div>
            <MenuButton/>
            <h1>E-Store Repair Job Form</h1>
            <form onSubmit={addJob} >
                <label htmlFor="name">Name</label>
                <input name="name" />
                <label htmlFor="phone">Phone number</label>
                <input name="phone" placeholder="required" />
                <label htmlFor="phone2">Secondary phone number</label>
                <input name="phone2" />
                <label htmlFor="email">Email</label>
                <input name="email" />
                <label htmlFor="address">Address</label>
                <input name="address" />
                <label htmlFor="Device">Device</label>
                <input name="device" />
                <label htmlFor="notes">Notes</label>
                <textarea name="notes" />
                <label htmlFor="price">Estimated price</label>
                <input name="price" />
                <button type="submit">Submit</button>
            </form>
        </div>
    );
}

export default FormPage;