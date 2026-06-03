import { toast } from '@/components/CustomToast';
import axios from "axios";

// follow OutCustomerDTO
export type Customer = {
    id: string
    name: string
    phone: string
    secondPhone: string
    email: string
    address: string
}

function _mapCustomer(c: { customerId: string; customerName: string; phoneNumber: string; phoneNumberSecondary: string; email: string; address: string }): Customer {
    return {
        id: c.customerId,
        name: c.customerName,
        phone: c.phoneNumber,
        secondPhone: c.phoneNumberSecondary,
        email: c.email,
        address: c.address
    };
}

export async function getCustomer(id: string): Promise<Customer> {
    try {
        const response = await axios.get(`api/Customers/${id}`);
        return _mapCustomer(response.data);
    } catch (error) {
        let message;

        if (axios.isAxiosError(error)) {
            const data = error.response?.data;
            const text = typeof data === 'string' ? data : null;

            if (error.response?.status === 404) {
                message = text ?? "Customer not found.";
            }
        }

        toast.error(message ?? "Something went wrong.");
        throw error;
    }
}