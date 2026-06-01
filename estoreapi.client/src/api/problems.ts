import axios from 'axios';
import { Problem } from '@/pages/devices/components/DeviceEdit';

export async function getProblems(deviceId: string): Promise<Problem[]> {
    const response = await axios.get(`/api/problems/device/${deviceId}`);
    return response.data.map((d: { problemId: string; problemName: string; price: string }) => ({
        id: d.problemId,
        name: d.problemName,
        price: parseFloat(d.price)
    }));
}