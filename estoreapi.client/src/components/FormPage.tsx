import MenuButton from './MenuButton';

function FormPage() {
    return (
        <div>
            <MenuButton/>
            <h1>E-Store Repair Job Form</h1>
            <form>
                <label htmlFor="name">Name</label>
                <input name="name" />
                <label htmlFor="phone">Phone number</label>
                <input name="phone" />
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