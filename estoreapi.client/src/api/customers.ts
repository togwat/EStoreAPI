import { api } from './client';
import { handleApiError } from './apiHelpers';

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

export async function getCustomers(): Promise<Customer[]> {
    const response = await api.get('/api/Customers');
    return response.data.map(_mapCustomer);
}

export async function getCustomer(id: string): Promise<Customer> {
    try {
        const response = await api.get(`/api/Customers/${id}`);
        return _mapCustomer(response.data);
    } catch (error) {
        handleApiError(error, { 404: "Customer not found." }, "Couldn't load customer");
    }
}
