import { useEffect, useState } from 'react';
import { useIsMobile } from '@/hooks/use-mobile';
import { PanelDrawer } from '@/components/PanelDrawer';
import { WorkingPagination } from '@/components/WorkingPagination';
import { DeviceCard } from './components/DeviceCard';
import ProblemEdit from './components/ProblemEdit';
import { Filter, FilterSearch, FilterSelect } from '@/components/Filter';
import { Device, getDevices, getDeviceTypes } from '@/api/devices';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { PencilIcon, PlusIcon, X } from 'lucide-react';

export default function DevicesPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [devices, setDevices] = useState<Device[]>([]);
    const [deviceTypes, setDeviceTypes] = useState<string[]>([]);
    const [selectedDevice, setSelectedDevice] = useState<Device | null>(null);
    const [selectedType, setSelectedType] = useState('all');
    const [isEditing, setIsEditing] = useState(false);
    const [page, setPage] = useState(1);

    useEffect(() => {
        getDevices().then(setDevices);
        getDeviceTypes().then(setDeviceTypes);
    }, []);

    // reset edit mode when the selected device changes
    useEffect(() => { setIsEditing(false); }, [selectedDevice?.id]);

    // reset to page 1 whenever the filter or layout changes
    useEffect(() => { setPage(1); }, [selectedType]);
    useEffect(() => { setPage(1); }, [isMobile]);
    const itemsPerPage = isMobile ? 10 : 32;

    const filteredDevices = selectedType !== 'all' ? devices.filter(d => d.type === selectedType) : devices;
    const pagedDevices = filteredDevices.slice((page - 1) * itemsPerPage, page * itemsPerPage);

    const cards = pagedDevices.map(device => (
        <DeviceCard key={device.id} device={device} isSelected={selectedDevice?.id === device.id} onClick={() => setSelectedDevice(device)} />
    ));

    const pagination = <WorkingPagination className="mt-4" page={page} totalItems={devices.length} itemsPerPage={itemsPerPage} onPageChange={setPage} />;

    return (
        <PanelDrawer
            open={selectedDevice !== null}
            drawerContent={selectedDevice && (
                <div className="w-full h-full overflow-auto">
                    {/** header */}
                    <div className={`flex items-center justify-between ${isMobile ? "p-4" : "pb-4"} border-b`}>
                        <div className="flex items-center justify-start gap-2">
                            {isEditing ?
                                <div className="flex flex-row justify-start gap-2">
                                    <Input className="w-28" placeholder={selectedDevice.name} />
                                    <Input className="w-28" list="type-datalist" placeholder={selectedDevice.type} />
                                    <datalist id="type-datalist">
                                        {deviceTypes.map(type => (
                                            <option value={type} />
                                        ))}
                                    </datalist>
                                </div>
                            :
                                <div className="flex flex-row items-end justify-start gap-2">
                                    <span className="text-base font-medium">{selectedDevice.name}</span>
                                    <span className="text-sm text-muted-foreground">{selectedDevice.type}</span>
                                    <Button variant="ghost" size="icon" onClick={() => setIsEditing(true)}><PencilIcon /></Button>
                                </div>
                            }
                        </div>
                        <Button variant="outline" size="icon" onClick={() => setSelectedDevice(null)}><X /></Button>
                    </div>

                    <ProblemEdit deviceId={selectedDevice.id} isEditing={isEditing} onEditingChange={setIsEditing} />
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
                    {pagination}
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
                    <span className="pl-1 text-muted-foreground">{filteredDevices.length} results</span>
                    <div className="py-4 grid grid-cols-[repeat(auto-fill,_minmax(18rem,_1fr))] gap-4">{cards}</div>
                    {pagination}
                  </div>
            }
        </PanelDrawer>
    );
}
