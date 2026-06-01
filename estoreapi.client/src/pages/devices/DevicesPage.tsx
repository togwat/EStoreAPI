import { useEffect, useState } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { DeviceCard, Device } from './components/DeviceCard';
import DeviceEdit from './components/DeviceEdit';
import axios from 'axios';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { getDeviceTypes } from '@/api/devices';
import { Button } from '@/components/ui/button';
import { X } from 'lucide-react';

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
    const [deviceTypes, setDeviceTypes] = useState<string[]>([]);
    const [selectedDevice, setSelectedDevice] = useState<Device | null>(null);
    const [selectedType, setSelectedType] = useState('all');

    useEffect(() => {
        getDevices().then(setDevices);
        getDeviceTypes().then(setDeviceTypes);
    }, []);

    const filteredDevices = selectedType !== 'all' ? devices.filter(d => d.type === selectedType) : devices;

    const cards = filteredDevices.map(device => (
        <DeviceCard key={device.id} device={device} isSelected={selectedDevice?.id === device.id} onClick={() => setSelectedDevice(device)} />
    ))

    return (
        <PanelDrawer
            open={selectedDevice !== null}
            drawerContent={selectedDevice && (
                <div className="w-full h-full overflow-auto">
                    {/** header */}
                    <div className={`flex items-center justify-between ${isMobile ? "p-4" : "pb-4"} border-b`}>
                        <span className="text-base font-medium">{selectedDevice.name}</span>
                        <Button variant="outline" size="icon" onClick={() => setSelectedDevice(null)}><X /></Button>
                    </div>
                    
                    <DeviceEdit deviceId={selectedDevice.id} />
                </div>
            )}
        >
            {isMobile
                // mobile 1 column layout
                ? <div className="flex flex-col gap-2">{cards}</div>
                // desktop grid layout
                : <div className="p-8">
                    <h1>{title}</h1>
                    <Filter>
                        <FilterSearch placeholder="Search devices..." />
                        <FilterSelect label="Device type" options={deviceTypes} value={selectedType} onChange={setSelectedType} />
                    </Filter>
                    <div className="py-4 grid grid-cols-[repeat(auto-fill,_minmax(18rem,_1fr))] gap-4">{cards}</div>
                  </div>
            }
        </PanelDrawer>
    );
}