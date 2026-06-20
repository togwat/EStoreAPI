import axios from "axios";
import { toast } from '@/components/CustomToast';
import { Problem } from "./problems";
import { api } from './client';

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
    estimatedPrice: string | null
    collectedPrice: string | null
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
    problems: { problemId: number; problemName: string; deviceId: number; price: number; labourPrice: number; riskCost: number }[];
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
            price: p.price,
            labourPrice: p.labourPrice,
            riskCost: p.riskCost
        })),
        estimatedPrice: j.estimatedPrice != null ? String(j.estimatedPrice) : null,
        collectedPrice: j.collectedPrice != null ? String(j.collectedPrice) : null,
        isFinished: j.isFinished,
    };
}


export async function getJobs(): Promise<Job[]> {
    const response = await api.get('/api/Jobs');
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

// follow InJobDTO
type UpdateJobPayload = {
    customerId: number;
    deviceId: number;
    receiveTime?: string | null;
    pickupTime?: string | null;
    estimatedPickupTime?: string | null;
    note?: string | null;
    problemIds: number[];
    estimatedPrice?: number | null;
    collectedPrice?: number | null;
    isFinished: boolean;
};

export async function updateJob(jobId: string, job: Job): Promise<void> {
    const body: UpdateJobPayload = {
        customerId: parseInt(job.customerId),
        deviceId: parseInt(job.deviceId),
        receiveTime: job.receiveTime,
        pickupTime: job.pickupTime,
        estimatedPickupTime: job.estimatedPickupTime,
        note: job.note,
        problemIds: job.problems.map(p => parseInt(p.id)),
        estimatedPrice: job.estimatedPrice ? parseFloat(job.estimatedPrice) : null, 
        collectedPrice: job.collectedPrice ? parseFloat(job.collectedPrice) : null,
        isFinished: job.isFinished,
    };

    try {
        await axios.put(`/api/Jobs/update/${jobId}`, body);
    } catch (error) {
        let message;

        if (axios.isAxiosError(error)) {
            const data = error.response?.data;
            const text = typeof data === 'string' ? data : null;

            if (error.response?.status === 404) {
                message = text ?? "Job not found.";
            } else if (error.response?.status === 400) {
                message = text ?? "One or more validation errors occurred.";
            }
        }

        toast.error(message ?? "Something went wrong.");
    }
}

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
