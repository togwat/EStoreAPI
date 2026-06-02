import axios from "axios";

type SubmitJobPayload = {
    name?: string;
    phoneNumber: string;
    phoneNumberSecondary?: string;
    email?: string;
    address?: string;
    deviceName?: string;
    problems: string[];
    estimatedPickupTime?: string | null;
    estimatedPrice?: number | null;
    note?: string;
};

export async function submitJob(payload: SubmitJobPayload): Promise<{ jobId: number }> {
    try {
        const response = await axios.post<{ jobId: number }>('/api/Form/submit', payload);
        return response.data;
    } catch (error) {
        if (axios.isAxiosError(error)) {
            const data = error.response?.data;
            const message = typeof data === 'string' ? data : (data?.title ?? error.message);
            throw new Error(message);
        }
        throw error;
    }
}