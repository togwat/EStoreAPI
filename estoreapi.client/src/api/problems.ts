import axios from 'axios';
import { toast } from '@/components/CustomToast';
import { api } from './client';

// follow OutProblemDTO
export type Problem = {
    id: string
    name: string
    price: number
    labourPrice: number
}

function _mapProblem(d: { problemId: string; problemName: string; price: string; labourPrice: string }): Problem {
    return {
        id: String(d.problemId),
        name: d.problemName,
        price: parseFloat(d.price),
        labourPrice: parseFloat(d.labourPrice)
    };
}

export async function getProblems(deviceId: string): Promise<Problem[]> {
    const response = await api.get(`/api/problems/device/${deviceId}`);
    return response.data.map(_mapProblem);
}

export async function updateProblems(deviceId: string, problems: Problem[]) {
    // InProblemDTO body
    const body = problems.map(p => ({
        problemId: p.id ? parseInt(p.id) : null,    // null id means to add
        problemName: p.name,
        deviceId: parseInt(deviceId),
        price: p.price,
        labourPrice: p.labourPrice
    }));

    try {
        await axios.put(`/api/problems/device/${deviceId}`, body);
    } catch (error) {
        let message;

        if (axios.isAxiosError(error)) {
            const data = error.response?.data;
            const text = typeof data === 'string' ? data : null;

            if (error.response?.status === 409) {
                message = text ?? "One or more problems are in use by a job and cannot be deleted.";
            } else if (error.response?.status === 404) {
                message = text ?? "Device not found.";
            } else if (error.response?.status === 400) {
                message = text ?? "One or more validation errors occurred.";
            }
        }

        toast.error(message ?? "Something went wrong.");
    }
}