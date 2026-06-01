import axios from 'axios';

export async function getDeviceTypes(): Promise<string[]> {
    const res = await axios.get<string[]>('/api/Devices/types');
    return res.data;
}
