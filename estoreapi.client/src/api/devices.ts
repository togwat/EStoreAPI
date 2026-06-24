import { toast } from '@/components/CustomToast';
import { api } from './client';
import { handleApiError } from './apiHelpers';

// follow OutDeviceDTO
export type Device = {
    id: string
    name: string
    modelNumber: string
    type: string
}

function _mapDevice(d: { deviceId: string; deviceName: string; modelNumber: string; deviceType: string }): Device {
    return {
        id: d.deviceId,
        name: d.deviceName,
        modelNumber: d.modelNumber,
        type: d.deviceType,
    };
}

export async function getDevice(id: string): Promise<Device> {
    try {
        const response = await api.get(`/api/Devices/${id}`);
        return _mapDevice(response.data);
    } catch (error) {
        handleApiError(error, { 404: "Device not found." }, "Couldn't load device");
    }
}

export async function getDevices(): Promise<Device[]> {
    const response = await api.get('/api/devices');
    return response.data.map(_mapDevice);
}

export async function getDeviceTypes(): Promise<string[]> {
    const response = await api.get<string[]>('/api/Devices/types');
    return response.data;
}

export async function searchDeviceType(type: string): Promise<Device[]> {
    const response = await api.get('/api/devices/searchType', { params: { type } })
    return response.data.map(_mapDevice);
}

// follow InDeviceDTO
type CreateDevicePayload = {
    deviceName: string;
    modelNumber?: string | null;
    deviceType: string;
};

// follow UpdateDeviceDTO
type UpdateDevicePayload = {
    deviceName?: string | null;
    modelNumber?: string | null;
    deviceType?: string | null;
};

export async function addDevice(device: Device): Promise<Device> {
    const payload: CreateDevicePayload = {
        deviceName: device.name,
        modelNumber: device.modelNumber,
        deviceType: device.type,
    }

    try {
        const response = await api.post('/api/Devices/create', payload);
        toast.success("Device created", `${response.data.deviceName} with id ${response.data.deviceId}`);
        return _mapDevice(response.data);
    } catch (error) {
        handleApiError(error, { 400: "One or more validation errors occurred." }, "Couldn't create device");
    }
}

export async function updateDevice(id: string, device: Device): Promise<void> {
    const body: UpdateDevicePayload = {
        deviceName: device.name,
        modelNumber: device.modelNumber,
        deviceType: device.type,
    }

    try {
        await api.put(`/api/Devices/update/${id}`, body);
    } catch (error) {
        handleApiError(error, {
            404: "Device not found.",
            400: "One or more validation errors occurred.",
        }, "Couldn't update device");
    }
}
