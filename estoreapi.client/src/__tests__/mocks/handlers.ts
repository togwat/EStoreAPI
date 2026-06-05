import { http, HttpResponse } from 'msw'
import { jobFixtures, customerFixtures, deviceFixtures, deviceTypeFixtures, problemFixtures } from './fixtures'

export const handlers = [
    http.get('/api/Jobs', () => HttpResponse.json(jobFixtures)),

    http.put('/api/Jobs/update/:id', () => new HttpResponse(null, { status: 200 })),

    http.post('/api/Form/submit', () => HttpResponse.json({ jobId: 42 })),

    http.get('/api/devices', () => HttpResponse.json(deviceFixtures)),

    http.get('/api/Devices/types', () => HttpResponse.json(deviceTypeFixtures)),

    http.get('/api/devices/searchType', ({ request }) => {
        const type = new URL(request.url).searchParams.get('type')
        const filtered = deviceFixtures.filter(d => d.deviceType === type)
        return HttpResponse.json(filtered)
    }),

    http.get('/api/Devices/:id', ({ params }) => {
        const device = deviceFixtures.find(d => d.deviceId === params.id)
        if (!device) return new HttpResponse(null, { status: 404 })
        return HttpResponse.json(device)
    }),

    http.post('/api/Devices/create', async ({ request }) => {
        const body = await request.json() as { deviceName: string; deviceType: string }
        return HttpResponse.json({ deviceId: '99', deviceName: body.deviceName, deviceType: body.deviceType })
    }),

    http.put('/api/Devices/update/:id', () => new HttpResponse(null, { status: 200 })),

    http.get('/api/problems/device/:id', ({ params }) => {
        const problems = problemFixtures[params.id as string] ?? []
        return HttpResponse.json(problems)
    }),

    http.put('/api/problems/device/:id', () => new HttpResponse(null, { status: 200 })),

    // customers.ts uses 'api/Customers' without a leading slash — resolves to /api/Customers in the browser
    http.get('/api/Customers', () => HttpResponse.json(customerFixtures)),

    http.get('/api/Customers/:id', ({ params }) => {
        const customer = customerFixtures.find(c => String(c.customerId) === params.id)
        if (!customer) return new HttpResponse(null, { status: 404 })
        return HttpResponse.json(customer)
    }),
]
