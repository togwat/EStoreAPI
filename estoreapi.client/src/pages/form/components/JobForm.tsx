import { useEffect, useRef, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Device, getDevices, getDeviceTypes, searchDeviceType } from '@/api/devices';
import { Problem, getProblems } from '@/api/problems';
import { submitJob } from '@/api/jobs';
import { toast } from '@/components/CustomToast';
import { toLocalDatetimeInputValue } from '@/lib/toLocalDatetime';

// for estimated pickup date, which for now is today + 1
function getTomorrow(): string {
    const d = new Date();
    d.setDate(d.getDate() + 1);
    return toLocalDatetimeInputValue(d.toISOString());
}

export default function JobForm() {
    const [deviceTypes, setDeviceTypes] = useState<string[]>([]);
    const [selectedType, setSelectedType] = useState('');
    const [deviceSuggestions, setDeviceSuggestions] = useState<Device[]>([]);
    const [selectedDeviceId, setSelectedDeviceId] = useState<string | null>(null);
    const [problemSuggestions, setProblemSuggestions] = useState<Problem[]>([]);
    const [problems, setProblems] = useState<string[]>(['']);
    const [submitting, setSubmitting] = useState(false);

    const deviceInputRef = useRef<HTMLInputElement>(null);

    // get device types for select input
    useEffect(() => {
        getDeviceTypes().then(setDeviceTypes);
    }, []);

    // get device models for model suggestions
    useEffect(() => {
        setSelectedDeviceId(null);
        setProblems(['']);
        if (deviceInputRef.current) deviceInputRef.current.value = '';
        // return all devices if no type is selected
        if (!selectedType) {
            getDevices().then(setDeviceSuggestions);
        }
        // get models for a specific type
        else {
            searchDeviceType(selectedType).then(setDeviceSuggestions);
        }
    }, [selectedType]);

    // get problems of device for problem suggestions
    useEffect(() => {
        if (selectedDeviceId === null) {
            setProblemSuggestions([]);
            return;
        }
        getProblems(selectedDeviceId).then(setProblemSuggestions);
    }, [selectedDeviceId]);

    // set new device id and reset problem suggestions, fetching new ones
    function handleDeviceChange(e: React.ChangeEvent<HTMLInputElement>) {
        // find device from device type suggestions
        const match = deviceSuggestions.find(d => d.name.toLowerCase().trim() === e.target.value.toLowerCase().trim());
        setSelectedDeviceId(match?.id ?? null);
        setProblems(['']);
    }

    // for any problem field, if changed:
    // replace only the problem at the given index with the new value, leaving all others unchanged
    function handleProblemChange(index: number, value: string) {
        setProblems(prev => prev.map((p, i) => i === index ? value : p));
    }

    // control for adding/removing problem fields
    function addProblem() {
        setProblems(prev => [...prev, '']);
    }

    function removeProblem() {
        if (problems.length > 1) setProblems(prev => prev.slice(0, -1));
    }

    // Suggest estimated price as sum of inputted problem prices
    const problemPriceMap = new Map(problemSuggestions.map(p => [p.name.toLowerCase().trim(), p.price + p.labourPrice]));
    const estimatedPrice = problems.reduce((sum, name) => sum + (problemPriceMap.get(name.toLowerCase().trim()) ?? 0), 0);

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        // retrieve all form data
        const formData = new FormData(event.currentTarget);

        const name = formData.get("name")?.toString().trim();
        const phone = formData.get("phone")!.toString().trim();
        const phone2 = formData.get("phone2")?.toString().trim();
        const email = formData.get("email")?.toString().trim();
        const address = formData.get("address")?.toString().trim();
        const device = formData.get("device")?.toString().trim();
        // filter for non-empty problems
        const problemNames = problems.filter(p => p.trim() !== '');
        // convert to postgre friendly format
        const pickupRaw = formData.get("pickup")?.toString();
        const estPickupDate = pickupRaw ? new Date(pickupRaw).toISOString() : null;
        // convert to js number, or null if empty
        const estPrice = parseFloat(formData.get("price")?.toString() ?? '') || null;
        const notes = formData.get("notes")?.toString().trim();

        const form = event.currentTarget;
        setSubmitting(true);
        try {
            const result = await submitJob({
                name,
                phoneNumber: phone,
                phoneNumberSecondary: phone2,
                email,
                address,
                deviceName: device,
                problems: problemNames,
                estimatedPickupTime: estPickupDate,
                estimatedPrice: estPrice,
                note: notes,
            });

            // reset form
            form.reset();
            setSelectedType('');

            toast.success("Success", `Job ${result.jobId} has been created.`);
        } catch (error) {
            toast.error("Submission failed", error instanceof Error ? error.message : 'An unexpected error occurred.');
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <form className="flex flex-col gap-4 max-w-lg mx-auto" onSubmit={handleSubmit}>
            <FieldGroup>
                <Field>
                    <FieldLabel htmlFor="name">Name</FieldLabel>
                    <Input id="name" name="name" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="phone" className="after:content-['_*'] after:text-destructive">Phone number</FieldLabel>
                    <Input id="phone" name="phone" placeholder="required" required />
                </Field>
                <Field>
                    <FieldLabel htmlFor="phone2">Secondary phone number</FieldLabel>
                    <Input id="phone2" name="phone2" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="email">Email</FieldLabel>
                    <Input id="email" name="email" type="email" />
                </Field>
                <Field>
                    <FieldLabel htmlFor="address">Address</FieldLabel>
                    <Input id="address" name="address" />
                </Field>
                <Field>
                    <FieldLabel>Device type</FieldLabel>
                    <Select value={selectedType} onValueChange={setSelectedType}>
                        <SelectTrigger className="w-full">
                            <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                            {deviceTypes.map(type => (
                                <SelectItem key={type} value={type}>{type}</SelectItem>
                            ))}
                        </SelectContent>
                    </Select>
                </Field>
                <Field>
                    <FieldLabel htmlFor="device" className="after:content-['_*'] after:text-destructive">Device model</FieldLabel>
                    <Input id="device" name="device" list="device-datalist" placeholder="required" required ref={deviceInputRef} onChange={handleDeviceChange} />
                    <datalist id="device-datalist">
                        {deviceSuggestions.map(d => (
                            <option key={d.id} value={d.name} />
                        ))}
                    </datalist>
                </Field>
                <Field>
                    <FieldLabel className="after:content-['_*'] after:text-destructive">Problems</FieldLabel>
                    <datalist id="problem-datalist">
                        {problemSuggestions.map(p => (
                            <option key={p.id} value={p.name} />
                        ))}
                    </datalist>
                    <div className="flex flex-col gap-2">
                        {problems.map((problem, i) => (
                            <Input key={i} name="problem[]" list="problem-datalist"
                                placeholder={i === 0 ? "required" : undefined}
                                required={i === 0}
                                value={problem}
                                onChange={e => handleProblemChange(i, e.target.value)}
                            />
                        ))}
                    </div>
                    <div className="flex gap-2 mt-2 justify-evenly">
                        <Button type="button" variant="outline" size="sm" onClick={addProblem}>Add problem</Button>
                        <Button type="button" variant="outline" size="sm" onClick={removeProblem} disabled={problems.length <= 1}>Remove problem</Button>
                    </div>
                </Field>
                <Field>
                    <FieldLabel htmlFor="price">Estimated price</FieldLabel>
                    <Input id="price" name="price" type="number" placeholder={String(estimatedPrice)} />
                </Field>
                <Field>
                    <FieldLabel htmlFor="pickup">Estimated pickup date</FieldLabel>
                    <Input id="pickup" name="pickup" type="datetime-local" min={new Date().toISOString().split('T')[0]} defaultValue={getTomorrow()} />
                </Field>
                <Field>
                    <FieldLabel htmlFor="notes">Notes</FieldLabel>
                    <Textarea id="notes" name="notes" />
                </Field>
            </FieldGroup>
            <Button type="submit" disabled={submitting}>
                {submitting ? 'Submitting...' : 'Submit'}
            </Button>
        </form>
    );
}
