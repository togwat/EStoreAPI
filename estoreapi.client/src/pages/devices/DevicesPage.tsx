import { useEffect, useState } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { columns, Device } from './components/DeviceColumns';
import axios from 'axios';
import { DataTable } from '@/components/ui/data-table';

async function getDevices(): Promise<Device[]> {
    const response = await axios.get('/api/devices');
    return response.data.map((d: { deviceId: string; deviceName: string; deviceType: string }) => ({
        id: d.deviceId,
        name: d.deviceName,
        type: d.deviceType,
    }));
}

export default function DevicesPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [devices, setDevices] = useState<Device[]>([]);

    useEffect(() => {
        getDevices().then(setDevices);
    }, []);

    return (
        <div>
            { !isMobile && <h1>{title}</h1> }
            <DataTable columns={columns} data={devices} />
        </div>
    );
}