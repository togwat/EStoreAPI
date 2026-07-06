import { toast } from '@/components/CustomToast';
import { Problem } from "./problems";
import { api } from './client';
import { handleApiError } from './apiHelpers';

// follow OutJobDTO
export type Job = {
    jobId: string
    customerId: string
    deviceId: string
    receiveTime: string
    pickupTime: string
    estimatedPickupTime?: string | null
    note: string
    problems: Problem[]
    estimatedPrice?: number | null
    collectedPrice?: number | null
    isFinished: boolean
    warrantyOfJobId?: string | null
}

function _mapJob(j: {
    jobId: number;
    customerId: number;
    deviceId: number;
    receiveTime: string;
    pickupTime: string;
    estimatedPickupTime: string;
    note: string;
    problems: { problemId: number; problemName: string; deviceId: number; price: number; partsPrice: number; labourPrice: number; riskCost: number }[];
    estimatedPrice: number;
    collectedPrice: number;
    isFinished: boolean;
    warrantyOfJobId: number;
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
            partsPrice: p.partsPrice,
            labourPrice: p.labourPrice,
            riskCost: p.riskCost
        })),
        estimatedPrice: j.estimatedPrice ?? null,
        collectedPrice: j.collectedPrice ?? null,
        isFinished: j.isFinished,
        warrantyOfJobId: j.warrantyOfJobId != null ? String(j.warrantyOfJobId) : null,
    };
}


export async function getJobs(): Promise<Job[]> {
    const response = await api.get('/api/Jobs');
    return response.data.map(_mapJob);
}

// follow job form
type SubmitJobPayload = {
    name?: string;
    primaryContact: string;
    phoneNumber?: string;
    email?: string;
    address?: string;
    deviceName?: string;
    problems: string[];
    estimatedPickupTime?: string | null;
    estimatedPrice?: number | null;
    note?: string;
};

// follow InJobDTO
type CreateJobPayload = {
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
    warrantyOfJobId?: number | null;
};

// follow UpdateJobDTO
type UpdateJobPayload = {
    pickupTime?: string | null;
    estimatedPickupTime?: string | null;
    note?: string | null;
    problemIds: number[];
    estimatedPrice?: number | null;
    collectedPrice?: number | null;
    isFinished: boolean;
    warrantyOfJobId?: number | null;
};

export async function addJob(job: Job): Promise<Job> {
    const payload: CreateJobPayload = {
        customerId: parseInt(job.customerId),
        deviceId: parseInt(job.deviceId),
        receiveTime: job.receiveTime || null,
        pickupTime: job.pickupTime || null,
        estimatedPickupTime: job.estimatedPickupTime || null,
        note: job.note || null,
        problemIds: job.problems.map(p => parseInt(p.id)),
        estimatedPrice: job.estimatedPrice ?? null,
        collectedPrice: job.collectedPrice ?? null,
        isFinished: job.isFinished,
        warrantyOfJobId: job.warrantyOfJobId ? parseInt(job.warrantyOfJobId) : null,
    };

    try {
        const response = await api.post('/api/Jobs/create', payload);
        toast.success("Job created", `Id: #${response.data.jobId}`);
        return _mapJob(response.data);
    } catch (error) {
        handleApiError(error, {
            400: "One or more validation errors occurred.",
            404: "Customer, device, or warranty job not found.",
        }, "Couldn't create job");
    }
}

export async function updateJob(jobId: string, job: Job): Promise<void> {
    const body: UpdateJobPayload = {
        pickupTime: job.pickupTime || null,
        estimatedPickupTime: job.estimatedPickupTime || null,
        note: job.note || null,
        problemIds: job.problems.map(p => parseInt(p.id)),
        estimatedPrice: job.estimatedPrice ?? null,
        collectedPrice: job.collectedPrice ?? null,
        isFinished: job.isFinished,
        warrantyOfJobId: job.warrantyOfJobId ? parseInt(job.warrantyOfJobId) : null,
    };

    try {
        await api.put(`/api/Jobs/update/${jobId}`, body);
    } catch (error) {
        handleApiError(error, {
            404: "Job not found.",
            400: "One or more validation errors occurred.",
        }, "Couldn't update job");
    }
}

export async function submitJob(payload: SubmitJobPayload): Promise<{ jobId: number }> {
    try {
        const response = await api.post<{ jobId: number }>('/api/Form/submit', payload);
        toast.success("Success", `Job ${response.data.jobId} has been created.`);
        return response.data;
    } catch (error) {
        handleApiError(error, {
            400: "Submission failed. Please check the form and try again.",
        }, "Submission failed");
    }
}
