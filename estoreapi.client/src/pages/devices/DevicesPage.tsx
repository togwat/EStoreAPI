import { useEffect, useState } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { DeviceCard, Device } from './components/DeviceCard';
import DeviceEdit from './components/DeviceEdit';
import axios from 'axios';
import Filter from '@/components/Filter';

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
    const [selectedDevice, setSelectedDevice] = useState<Device | null>(null);

    useEffect(() => {
        getDevices().then(setDevices);
    }, []);

    const cards = devices.map(device => (
        <DeviceCard key={device.id} device={device} isSelected={selectedDevice?.id === device.id} onClick={() => setSelectedDevice(device)} />
    ))

    return (
        <PanelDrawer
            open={selectedDevice !== null}
            onClose={() => setSelectedDevice(null)}
            title={selectedDevice?.name}
            drawerContent={selectedDevice && <DeviceEdit deviceId={selectedDevice.id} />}
        >
            {isMobile
                // mobile 1 column layout
                ? <div className="flex flex-col gap-2">{cards}</div>
                // desktop grid layout
                : <div className="p-8">
                    <h1>{title}</h1>
                    <Filter inputPlaceholder="Search devices..."/>
                    <div className="py-4 grid grid-cols-[repeat(auto-fill,_minmax(18rem,_1fr))] gap-4">{cards}</div>
                  </div>
            }
        </PanelDrawer>
    );
}