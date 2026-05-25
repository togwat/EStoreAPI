import axios from 'axios';

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
        const device = formData.get("device")?.toString().trim();
        const notes = formData.get("notes")?.toString().trim();
        // const price = formData.get("price")?.toString().trim(); // might get rid of this field
                
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
        console.log(`${name} ${phone} ${phone2} ${email} ${address}, ${device}, ${notes}`);
    }

    return (
        <form className="flex flex-col gap-4" onSubmit={handleSubmit}>
            <div className="flex flex-col gap-2">
                <label htmlFor="name" className="text-left">Name</label>
                <input name="name" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="phone" className="text-left after:content-['_*'] after:text-red-500">Phone number</label>
                <input name="phone" placeholder="required" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="phone2" className="text-left">Secondary phone number</label>
                <input name="phone2" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="email" className="text-left">Email</label>
                <input name="email" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="address" className="text-left">Address</label>
                <input name="address" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="device" className="text-left">Device</label>
                <input name="device" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="notes" className="text-left">Notes</label>
                <textarea name="notes" className="w-full" />
            </div>
            <div className="flex flex-col gap-2">
                <label htmlFor="price" className="text-left">Estimated price</label>
                <input name="price" className="w-full" />
            </div>
            <button type="submit">Submit</button>
        </form>
    )
}