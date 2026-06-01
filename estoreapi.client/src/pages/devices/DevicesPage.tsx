import { useEffect, useState } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { DeviceCard, Device } from './components/DeviceCard';
import DeviceEdit from './components/DeviceEdit';
import axios from 'axios';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { getDeviceTypes } from '@/api/devices';
import { Button } from '@/components/ui/button';
import { PencilIcon, PlusIcon, X } from 'lucide-react';

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
    const [isEditing, setIsEditing] = useState(false);

    useEffect(() => {
        getDevices().then(setDevices);
        getDeviceTypes().then(setDeviceTypes);
    }, []);

    // reset edit mode when the selected device changes
    useEffect(() => { setIsEditing(false); }, [selectedDevice?.id]);

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
                        <div className="flex items-center justify-start gap-2">
                            <span className="text-base font-medium">{selectedDevice.name}</span>
                            {!isEditing && 
                                <Button variant="ghost" size="icon" onClick={() => setIsEditing(true)}><PencilIcon /></Button>
                            }
                        </div>
                        <Button variant="outline" size="icon" onClick={() => setSelectedDevice(null)}><X /></Button>
                    </div>

                    <DeviceEdit deviceId={selectedDevice.id} isEditing={isEditing} onEditingChange={setIsEditing} />
                </div>
            )}
        >
            {isMobile
                // mobile 1 column layout
                ? <div className="flex flex-col gap-2">
                    <div className="flex flex-row items-center justify-between">
                        <Filter>
                            <FilterSearch placeholder={`Search ${devices.length} devices...`} />
                            <FilterSelect label="Device type" options={deviceTypes} value={selectedType} onChange={setSelectedType} />
                        </Filter>
                        <Button size="icon-lg"><PlusIcon /></Button>
                    </div>
                    {cards}
                </div>
                // desktop grid layout
                : <div className="p-8">
                    <h1>{title}</h1>
                    <div className="flex flex-row items-center justify-between">
                        <Filter>
                            <FilterSearch placeholder="Search devices..." />
                            <FilterSelect label="Device type" options={deviceTypes} value={selectedType} onChange={setSelectedType} />
                        </Filter>
                        <Button size="lg"><PlusIcon />Add device</Button>
                    </div>
                    <span className="pl-1 text-muted-foreground">{devices.length} results</span>
                    <div className="py-4 grid grid-cols-[repeat(auto-fill,_minmax(18rem,_1fr))] gap-4">{cards}</div>
                  </div>
            }
        </PanelDrawer>
    );
}
