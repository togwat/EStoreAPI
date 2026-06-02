import axios, { AxiosResponse } from 'axios';

// follow OutDeviceDTO
export type Device = {
    id: string
    name: string
    type: string
}

function _mapDevice(response: AxiosResponse) {
    return response.data.map((d: { deviceId: string; deviceName: string; deviceType: string }) => ({
        id: d.deviceId,
        name: d.deviceName,
        type: d.deviceType,
    }));
}

export async function getDevices(): Promise<Device[]> {
    const response = await axios.get('/api/devices');
    return _mapDevice(response);
}

export async function getDeviceTypes(): Promise<string[]> {
    const response = await axios.get<string[]>('/api/Devices/types');
    return response.data;
}

export async function searchDeviceType(type: string): Promise<Device[]> {
    const response = await axios.get('/api/devices/searchType', { params: { type } })
    return _mapDevice(response);
}
