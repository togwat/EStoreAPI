import { api } from './client';
import { handleApiError } from './apiHelpers';

// follow OutProblemDTO
export type Problem = {
    id: string
    name: string
    price: number
    labourPrice: number
    riskCost: number
}

function _mapProblem(d: { problemId: string; problemName: string; price: string; labourPrice: string; riskCost: string }): Problem {
    return {
        id: String(d.problemId),
        name: d.problemName,
        price: parseFloat(d.price),
        labourPrice: parseFloat(d.labourPrice),
        riskCost: parseFloat(d.riskCost)
    };
}

export async function getProblems(deviceId: string): Promise<Problem[]> {
    const response = await api.get(`/api/problems/device/${deviceId}`);
    return response.data.map(_mapProblem);
}

// follow InProblemDTO
type ProblemPayload = {
    problemId: number | null;
    problemName: string;
    deviceId: number;
    price: number;
    labourPrice: number;
    riskCost: number;
};

export async function updateDeviceProblems(deviceId: string, problems: Problem[]): Promise<void> {
    const body: ProblemPayload[] = problems.map(p => ({
        problemId: p.id ? parseInt(p.id) : null,    // null id means to add
        problemName: p.name,
        deviceId: parseInt(deviceId),
        price: p.price,
        labourPrice: p.labourPrice,
        riskCost: p.riskCost
    }));

    try {
        await api.put(`/api/problems/device/${deviceId}`, body);
    } catch (error) {
        handleApiError(error, {
            409: "One or more problems are in use by a job and cannot be deleted.",
            404: "Device not found.",
            400: "One or more validation errors occurred.",
        }, "Couldn't update problems");
    }
}
