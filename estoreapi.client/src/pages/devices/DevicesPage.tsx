import { useEffect, useState } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { DeviceCard, Device } from './components/DeviceCard';
import axios from 'axios';

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

            {isMobile
                // mobile 1 column layout
                ? <div className="py-4 flex flex-col gap-4">
                    {devices.map(device => (
                        <DeviceCard key={device.id} device={device} />
                    ))}
                  </div>
                // desktop grid layout
                : <div className="py-4 grid grid-cols-[repeat(auto-fill,_minmax(18rem,_1fr))] gap-4">
                    {devices.map(device => (
                        <DeviceCard key={device.id} device={device} />
                    ))}
                  </div>
            }
        </div>
    );
}