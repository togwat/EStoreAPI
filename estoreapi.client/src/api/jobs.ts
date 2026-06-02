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
            throw new Error(error.response?.data ?? error.message);
        }
        throw error;
    }
}