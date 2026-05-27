import { Button } from '@/components/ui/button';
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
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
        <form className="flex flex-col gap-4 max-w-lg mx-auto" onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="name">Name</FieldLabel>
                    <Input id="name" name="name" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="phone" className="after:content-['_*'] after:text-destructive">Phone number</FieldLabel>
                    <Input id="phone" name="phone" placeholder="required" required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="phone2">Secondary phone number</FieldLabel>
                    <Input id="phone2" name="phone2" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="email">Email</FieldLabel>
                    <Input id="email" name="email" type="email" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="address">Address</FieldLabel>
                    <Input id="address" name="address" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="device">Device</FieldLabel>
                    <Input id="device" name="device" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="notes">Notes</FieldLabel>
                    <Textarea id="notes" name="notes" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="price">Estimated price</FieldLabel>
                    <Input id="price" name="price" />
                </Field>
            </FieldGroup>
            <Button type="submit">Submit</Button>
        </form>
    )
}