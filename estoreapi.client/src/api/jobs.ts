import axios  from "axios";
import { Problem } from "./problems";

// follow OutJobDTO
export type Job = {
    jobId: string
    customerId: string
    deviceId: string
    receiveTime: string
    pickupTime: string
    estimatedPickupTime: string
    note: string
    problems: Problem[]
    estimatedPrice: string
    collectedPrice: string
    isFinished: boolean
}

function _mapJob(j: {
    jobId: number;
    customerId: number;
    deviceId: number;
    receiveTime: string;
    pickupTime: string;
    estimatedPickupTime: string;
    note: string;
    problems: { problemId: number; problemName: string; deviceId: number; price: number }[];
    estimatedPrice: number;
    collectedPrice: number;
    isFinished: boolean;
}): Job {
    return {
        jobId: String(j.jobId),
        customerId: String(j.customerId),   // change Job type to customerId: string
        deviceId: String(j.deviceId),       // change Job type to deviceId: string
        receiveTime: j.receiveTime,
        pickupTime: j.pickupTime,
        estimatedPickupTime: j.estimatedPickupTime,
        note: j.note,
        problems: j.problems.map(p => ({
            id: String(p.problemId),
            name: p.problemName,
            price: p.price
        })),
        estimatedPrice: String(j.estimatedPrice),
        collectedPrice: String(j.collectedPrice),
        isFinished: j.isFinished,
    };
}


export async function getJobs(): Promise<Job[]> {
    const response = await axios.get('/api/Jobs');
    return response.data.map(_mapJob);
}

// follow job form
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