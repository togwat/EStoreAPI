import { toast } from '@/components/CustomToast';
import axios, { AxiosResponse } from 'axios';

// follow OutDeviceDTO
export type Device = {
    id: string
    name: string
    type: string
}

function _mapDevices(response: AxiosResponse) {
    return response.data.map((d: { deviceId: string; deviceName: string; deviceType: string }) => ({
        id: d.deviceId,
        name: d.deviceName,
        type: d.deviceType,
    }));
}

export async function getDevices(): Promise<Device[]> {
    const response = await axios.get('/api/devices');
    return _mapDevices(response);
}

export async function getDeviceTypes(): Promise<string[]> {
    const response = await axios.get<string[]>('/api/Devices/types');
    return response.data;
}

export async function searchDeviceType(type: string): Promise<Device[]> {
    const response = await axios.get('/api/devices/searchType', { params: { type } })
    return _mapDevices(response);
}

export async function addDevice(device: Device): Promise<Device> {
    const payload = {
        deviceName: device.name,
        deviceType: device.type,
    }

    try {
        const response = await axios.post("/api/Devices/create", payload);
        
        toast.success("Device created", `${response.data.deviceName} with id ${response.data.deviceId}`);

        return {
            id: response.data.deviceId,
            name: response.data.deviceName,
            type: response.data.deviceType,
        };
    } catch (error) {
        let message;

        if (axios.isAxiosError(error)) {
            const data = error.response?.data;
            const text = typeof data === 'string' ? data : null;

            if (error.response?.status === 400) {
                message = text ?? "One or more validation errors occurred.";
            }
        }

        toast.error(message ?? "Something went wrong.");
        throw error;
    }
}

export async function updateDevice(id: string, device: Device) {
    const body = {
        deviceName: device.name,
        deviceType: device.type,
    }

    try {
        await axios.put(`/api/Devices/update/${id}`, body);
    } catch (error) {
        let message;

        if (axios.isAxiosError(error)) {
            const data = error.response?.data;
            const text = typeof data === 'string' ? data : null;

            if (error.response?.status === 404) {
                message = text ?? "Device not found.";
            } else if (error.response?.status === 400) {
                message = text ?? "One or more validation errors occurred.";
            }
        }

        toast.error(message ?? "Something went wrong.");
    }
}