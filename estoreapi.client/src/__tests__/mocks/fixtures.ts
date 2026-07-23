// Backend DTO shapes — the same format the real API returns before mapping functions transform them

export const jobFixtures = [
    {
        jobId: 1,
        customerId: 1,
        deviceId: 1,
        receiveTime: '2024-01-01T08:00:00Z',
        pickupTime: null,
        estimatedPickupTime: '2024-01-15T17:00:00Z',
        note: 'Customer says screen cracked from drop',
        problems: [{ problemId: 1, problemName: 'Screen Replacement', deviceId: 1, price: 250 }],
        estimatedPrice: 250,
        collectedPrice: null,
        status: 'InProgress',
    },
    {
        jobId: 2,
        customerId: 2,
        deviceId: 2,
        receiveTime: '2024-01-03T10:00:00Z',
        pickupTime: '2024-01-10T14:30:00Z',
        estimatedPickupTime: null,
        note: null,
        problems: [{ problemId: 3, problemName: 'Battery Replacement', deviceId: 2, price: 80 }],
        estimatedPrice: 80,
        collectedPrice: 80,
        status: 'Finished',
    },
    {
        jobId: 3,
        customerId: 3,
        deviceId: 1,
        receiveTime: '2024-01-05T09:00:00Z',
        pickupTime: '2024-01-08T11:00:00Z',
        estimatedPickupTime: null,
        note: null,
        problems: [{ problemId: 1, problemName: 'Screen Replacement', deviceId: 1, price: 250 }],
        estimatedPrice: 250,
        collectedPrice: null,
        status: 'Cancelled',
    },
    {
        jobId: 4,
        customerId: 4,
        deviceId: 1,
        receiveTime: '2024-01-06T09:00:00Z',
        pickupTime: '2024-01-09T11:00:00Z',
        estimatedPickupTime: null,
        note: null,
        problems: [{ problemId: 1, problemName: 'Screen Replacement', deviceId: 1, price: 250 }],
        estimatedPrice: 250,
        collectedPrice: 250,
        status: 'Refunded',
    },
]

export const customerFixtures = [
    {
        customerId: 1,
        customerName: 'John Smith',
        primaryContact: '0211234567',
        phoneNumber: null,
        email: 'john@example.com',
        address: '123 Main St, Auckland',
    },
    {
        // primary contact is not necessarily a phone number (email, telegram id, etc.)
        customerId: 2,
        customerName: 'Jane Doe',
        primaryContact: 'jane@example.com',
        phoneNumber: '0279876543',
        email: null,
        address: null,
    },
    {
        customerId: 3,
        customerName: 'Smithy Jonny',
        primaryContact: '0273334444',
        phoneNumber: null,
        email: null,
        address: null,
    },
    {
        customerId: 4,
        customerName: 'Jinny Doey',
        primaryContact: '0274445555',
        phoneNumber: null,
        email: null,
        address: null,
    },
]

export const deviceFixtures = [
    { deviceId: '1', deviceName: 'iPhone 14', modelNumber: 'A2882', deviceType: 'Smartphone' },
    { deviceId: '2', deviceName: 'Samsung S22', modelNumber: 'SM-S901', deviceType: 'Smartphone' },
    { deviceId: '3', deviceName: 'MacBook Pro 2023', modelNumber: 'A2780', deviceType: 'Laptop' },
]

export const deviceTypeFixtures = ['Smartphone', 'Laptop', 'Tablet']

export const problemFixtures: Record<string, object[]> = {
    '1': [
        { problemId: '1', problemName: 'Screen Replacement', price: '250', labourPrice: '100', riskCost: '50', partsPrice: '35' },
        { problemId: '2', problemName: 'Battery Replacement', price: '80', labourPrice: '40', riskCost: '20', partsPrice: '18' },
    ],
    '2': [
        { problemId: '3', problemName: 'Battery Replacement', price: '80', labourPrice: '40', riskCost: '20', partsPrice: '18' },
        { problemId: '4', problemName: 'Charging Port Repair', price: '60', labourPrice: '30', riskCost: '15', partsPrice: '12' },
    ],
    '3': [
        { problemId: '5', problemName: 'SSD Replacement', price: '350', labourPrice: '120', riskCost: '70', partsPrice: '45' },
    ],
}
