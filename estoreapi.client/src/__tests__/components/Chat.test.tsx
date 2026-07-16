import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { render, screen, act } from '@testing-library/react'
import {
    AssistantRuntimeProvider,
    useLocalRuntime,
    ThreadPrimitive,
    MessagePrimitive,
    ErrorPrimitive,
} from '@assistant-ui/react'
import { agentAdapter, toOutgoingParts } from '@/components/chat/agentStream'
import { stripAttachmentFiles, remoteThreadListAdapter } from '@/components/chat/chatPersistence'
import { server } from '../mocks/server'
import { http, HttpResponse } from 'msw'

/**
 * Characterisation tests for Chat.tsx — these lock in the externally observable
 * behaviour of the agent stream adapter (how reasoning, tool calls, sources and
 * confirmation states are assembled for display) and the pure wire helpers, so a
 * subsequent refactor can be proven behaviour-preserving.
 */

// --- stream test harness ---------------------------------------------------

/** One NDJSON line for a stream event, including the trailing newline. */
const line = (event: unknown) => `${JSON.stringify(event)}\n`

/**
 * A minimal fetch Response whose body streams the given raw chunks. Each entry of
 * `chunks` is delivered in a single `read()`, letting tests exercise the line
 * buffering across arbitrary byte boundaries.
 */
function streamResponse(
    chunks: string[],
    { ok = true, statusText = 'OK', hasBody = true }: { ok?: boolean; statusText?: string; hasBody?: boolean } = {},
) {
    const encoder = new TextEncoder()
    let i = 0
    const body = {
        getReader: () => ({
            read: async () =>
                i < chunks.length
                    ? { done: false, value: encoder.encode(chunks[i++]) }
                    : { done: true, value: undefined },
        }),
    }
    return { ok, statusText, body: hasBody ? body : null }
}

/** Mock global fetch to return the given streamed response for /agent/chat. */
function mockAgentStream(response: ReturnType<typeof streamResponse>) {
    const fetchMock = vi.fn().mockResolvedValue(response)
    vi.stubGlobal('fetch', fetchMock)
    return fetchMock
}

type RunOptions = Parameters<typeof agentAdapter.run>[0]

/** Drive the adapter generator to completion and collect every yielded value. */
async function runAdapter(opts: Partial<RunOptions> = {}) {
    const full = {
        messages: [],
        abortSignal: undefined,
        unstable_getMessage: () => ({ content: [] }),
        ...opts,
    } as unknown as RunOptions
    const yields: Array<{ content: unknown[]; status?: { type: string; reason?: string } }> = []
    const stream = agentAdapter.run(full) as AsyncGenerator<unknown>
    for await (const value of stream) {
        yields.push(value as never)
    }
    return yields
}

/** The final yielded value — the completed message the runtime renders. */
const finalContent = (yields: Array<{ content: unknown[] }>) => yields[yields.length - 1].content

afterEach(() => vi.unstubAllGlobals())

// --- toOutgoingParts -------------------------------------------------------

describe('toOutgoingParts', () => {
    it('maps a text part to a text wire part', () => {
        expect(toOutgoingParts({ type: 'text', text: 'hi' })).toEqual([{ type: 'text', text: 'hi' }])
    })

    it('maps an image part to an image_url wire part', () => {
        expect(toOutgoingParts({ type: 'image', image: 'data:img' })).toEqual([
            { type: 'image_url', url: 'data:img' },
        ])
    })

    it('round-trips reasoning so thinking mode can re-receive it', () => {
        expect(toOutgoingParts({ type: 'reasoning', text: 'because' })).toEqual([
            { type: 'reasoning', text: 'because' },
        ])
    })

    it('maps a tool-call part, parsing argsText into structured input', () => {
        expect(
            toOutgoingParts({
                type: 'tool-call',
                toolCallId: 'c1',
                toolName: 'get_weather',
                argsText: '{"city":"NYC"}',
                result: 'sunny',
            }),
        ).toEqual([{ type: 'tool_use', id: 'c1', name: 'get_weather', input: { city: 'NYC' }, result: 'sunny' }])
    })

    it('drops unknown part types', () => {
        expect(toOutgoingParts({ type: 'something-else' })).toEqual([])
    })
})

// --- stripAttachmentFiles --------------------------------------------------

describe('stripAttachmentFiles', () => {
    it('returns the message unchanged when there are no attachments', () => {
        const message = { role: 'user', content: [] } as never
        expect(stripAttachmentFiles(message)).toBe(message)
    })

    it('returns the message unchanged when attachments is empty', () => {
        const message = { role: 'user', content: [], attachments: [] } as never
        expect(stripAttachmentFiles(message)).toBe(message)
    })

    it('drops the un-serializable File while preserving other attachment fields', () => {
        const message = {
            role: 'user',
            content: [],
            attachments: [
                { id: 'a1', type: 'image', file: new File(['x'], 'x.png'), content: [{ type: 'image', image: 'data:img' }] },
            ],
        } as never
        const result = stripAttachmentFiles(message) as unknown as {
            attachments: Array<Record<string, unknown>>
        }
        expect(result.attachments[0]).not.toHaveProperty('file')
        expect(result.attachments[0].id).toBe('a1')
        expect(result.attachments[0].content).toEqual([{ type: 'image', image: 'data:img' }])
    })
})

// --- agentAdapter.run: response part assembly ------------------------------

describe('agentAdapter.run — response parts', () => {
    it('accumulates text chunks into a single text part', async () => {
        mockAgentStream(streamResponse([line(['chunk', 'Hello ']), line(['chunk', 'world'])]))
        const content = finalContent(await runAdapter())
        expect(content).toEqual([{ type: 'text', text: 'Hello world' }])
    })

    it('accumulates reasoning and orders it before the text', async () => {
        mockAgentStream(streamResponse([line(['reasoning', 'let me think']), line(['chunk', 'answer'])]))
        const content = finalContent(await runAdapter())
        expect(content).toEqual([
            { type: 'reasoning', text: 'let me think' },
            { type: 'text', text: 'answer' },
        ])
    })

    it('renders a tool call and attaches its result, ordered between reasoning and text', async () => {
        mockAgentStream(
            streamResponse([
                line(['reasoning', 'thinking']),
                line(['tool_calls', [{ id: 't1', name: 'get_weather', arguments: { city: 'NYC' } }]]),
                line(['tool_result', 't1', 'sunny']),
                line(['chunk', 'It is sunny.']),
            ]),
        )
        const content = finalContent(await runAdapter())
        expect(content).toEqual([
            { type: 'reasoning', text: 'thinking' },
            {
                type: 'tool-call',
                toolCallId: 't1',
                toolName: 'get_weather',
                argsText: JSON.stringify({ city: 'NYC' }, null, 2),
                result: 'sunny',
            },
            { type: 'text', text: 'It is sunny.' },
        ])
    })

    it('splits a web_search result into a text result and source badges', async () => {
        mockAgentStream(
            streamResponse([
                line(['tool_calls', [{ id: 't1', name: 'web_search', arguments: { query: 'cats' } }]]),
                line([
                    'tool_result',
                    't1',
                    JSON.stringify({ text: 'cats are great', sources: [{ url: 'https://a.com', title: 'A' }] }),
                ]),
            ]),
        )
        const content = finalContent(await runAdapter()) as Array<Record<string, unknown>>
        const toolCall = content.find((p) => p.type === 'tool-call')
        const source = content.find((p) => p.type === 'source')
        expect(toolCall?.result).toBe('cats are great')
        expect(source).toEqual({
            type: 'source',
            sourceType: 'url',
            id: 't1-0',
            url: 'https://a.com',
            title: 'A',
        })
    })

    it('orders source badges after the text part', async () => {
        mockAgentStream(
            streamResponse([
                line(['tool_calls', [{ id: 't1', name: 'web_search', arguments: {} }]]),
                line(['tool_result', 't1', JSON.stringify({ text: 'r', sources: [{ url: 'https://a.com' }] })]),
                line(['chunk', 'final answer']),
            ]),
        )
        const content = finalContent(await runAdapter()) as Array<Record<string, unknown>>
        const types = content.map((p) => p.type)
        expect(types.indexOf('text')).toBeLessThan(types.indexOf('source'))
    })

    it('passes through a non-JSON web_search result with no sources', async () => {
        mockAgentStream(
            streamResponse([
                line(['tool_calls', [{ id: 't1', name: 'web_search', arguments: {} }]]),
                line(['tool_result', 't1', 'plain string result']),
            ]),
        )
        const content = finalContent(await runAdapter()) as Array<Record<string, unknown>>
        expect(content.find((p) => p.type === 'tool-call')?.result).toBe('plain string result')
        expect(content.some((p) => p.type === 'source')).toBe(false)
    })

    it('seeds web_search sources from an already-resolved tool call (confirmation-gated path)', async () => {
        const current = {
            content: [
                {
                    type: 'tool-call',
                    toolCallId: 't9',
                    toolName: 'web_search',
                    argsText: '{}',
                    result: JSON.stringify({ text: 'r', sources: [{ url: 'https://seed.com', title: 'Seed' }] }),
                },
            ],
        }
        mockAgentStream(streamResponse([line(['chunk', 'done'])]))
        const content = finalContent(await runAdapter({ unstable_getMessage: () => current as never })) as Array<
            Record<string, unknown>
        >
        expect(content).toContainEqual({
            type: 'source',
            sourceType: 'url',
            id: 't9-0',
            url: 'https://seed.com',
            title: 'Seed',
        })
    })
})

// --- agentAdapter.run: stream control & status -----------------------------

describe('agentAdapter.run — stream control', () => {
    it('marks the message requires-action when confirmation is required', async () => {
        mockAgentStream(
            streamResponse([
                line(['tool_calls', [{ id: 't1', name: 'delete_thing', arguments: {} }]]),
                line(['confirmation_required', ['t1']]),
            ]),
        )
        const yields = await runAdapter()
        expect(yields[yields.length - 1].status).toEqual({ type: 'requires-action', reason: 'tool-calls' })
    })

    it('does not set a status on a normal completion', async () => {
        mockAgentStream(streamResponse([line(['chunk', 'hi'])]))
        const yields = await runAdapter()
        expect(yields[yields.length - 1].status).toBeUndefined()
    })

    it("renders the backend's message in the error box when the stream ends with an error event", async () => {
        // End-to-end proof that an in-band ("error", message) event reaches the box:
        // drive a real local runtime with agentAdapter, then assert the ErrorPrimitive
        // renders the exact backend message rather than a generic network failure.
        mockAgentStream(
            streamResponse([
                line(['chunk', 'partial answer']),
                line(['error', 'error, boom']),
            ]),
        )

        // Minimal message view: assistant messages render only the error box.
        const ErrorBox = () => (
            <MessagePrimitive.Root>
                <MessagePrimitive.Error>
                    <ErrorPrimitive.Root>
                        <ErrorPrimitive.Message />
                    </ErrorPrimitive.Root>
                </MessagePrimitive.Error>
            </MessagePrimitive.Root>
        )
        let runtime: ReturnType<typeof useLocalRuntime> | undefined
        const Harness = () => {
            // Queue-enabled so the runtime swallows the run's re-thrown error internally
            // (it still sets the errored status first), keeping the floating rejection out of the test.
            runtime = useLocalRuntime(agentAdapter, { unstable_enableMessageQueue: true })
            return (
                <AssistantRuntimeProvider runtime={runtime}>
                    <ThreadPrimitive.Root>
                        <ThreadPrimitive.Messages
                            components={{ UserMessage: () => null, AssistantMessage: ErrorBox }}
                        />
                    </ThreadPrimitive.Root>
                </AssistantRuntimeProvider>
            )
        }

        render(<Harness />)
        // append enqueues the run (fire-and-forget); findByText waits for the error box to render.
        act(() => {
            runtime!.thread.append({ role: 'user', content: [{ type: 'text', text: 'hi' }] })
        })

        expect(await screen.findByText('error, boom')).toBeInTheDocument()
    })

    it('reassembles events split across read() boundaries', async () => {
        // The single NDJSON line is delivered in two separate reads.
        mockAgentStream(mockSplit('["chunk","buffered"]\n'))
        const content = finalContent(await runAdapter())
        expect(content).toEqual([{ type: 'text', text: 'buffered' }])
    })

    it('flushes a trailing line that arrives without a newline', async () => {
        mockAgentStream(streamResponse(['["chunk","a"]\n["chunk","b"]']))
        const content = finalContent(await runAdapter())
        expect(content).toEqual([{ type: 'text', text: 'ab' }])
    })

    it('ignores malformed JSON lines without throwing', async () => {
        mockAgentStream(streamResponse([line(['chunk', 'a']), 'this is not json\n', line(['chunk', 'b'])]))
        const content = finalContent(await runAdapter())
        expect(content).toEqual([{ type: 'text', text: 'ab' }])
    })
})

/** Split a complete payload into two reads at its midpoint. */
function mockSplit(payload: string) {
    const mid = Math.floor(payload.length / 2)
    return streamResponse([payload.slice(0, mid), payload.slice(mid)])
}

// --- agentAdapter.run: outgoing request assembly ---------------------------

describe('agentAdapter.run — request assembly', () => {
    it('formats message content and user attachments into the wire request', async () => {
        const fetchMock = mockAgentStream(streamResponse([line(['chunk', 'ok'])]))
        await runAdapter({
            messages: [
                {
                    role: 'user',
                    content: [{ type: 'text', text: 'hello' }],
                    attachments: [{ content: [{ type: 'image', image: 'data:img' }] }],
                },
            ] as never,
        })
        const body = JSON.parse(fetchMock.mock.calls[0][1].body)
        expect(body.stream).toBe(true)
        expect(body.messages).toEqual([
            {
                role: 'user',
                content: [
                    { type: 'text', text: 'hello' },
                    { type: 'image_url', url: 'data:img' },
                ],
            },
        ])
    })

    it('appends the in-progress assistant tool call so the backend can resume', async () => {
        const fetchMock = mockAgentStream(streamResponse([line(['chunk', 'ok'])]))
        const current = {
            content: [
                { type: 'tool-call', toolCallId: 't1', toolName: 'do_thing', argsText: '{"a":1}', result: 'r' },
            ],
        }
        await runAdapter({ unstable_getMessage: () => current as never })
        const body = JSON.parse(fetchMock.mock.calls[0][1].body)
        expect(body.messages).toContainEqual({
            role: 'assistant',
            content: [{ type: 'tool_use', id: 't1', name: 'do_thing', input: { a: 1 }, result: 'r' }],
        })
    })
})

// --- agentAdapter.run: error handling --------------------------------------

describe('agentAdapter.run — errors', () => {
    it('throws when the agent responds non-ok', async () => {
        mockAgentStream(streamResponse([], { ok: false, statusText: 'Bad Gateway' }))
        await expect(runAdapter()).rejects.toThrow('Agent error: Bad Gateway')
    })

    it('throws when there is no response body', async () => {
        mockAgentStream(streamResponse([], { hasBody: false }))
        await expect(runAdapter()).rejects.toThrow('No response body')
    })
})

// --- remoteThreadListAdapter: route mapping --------------------------------

describe('remoteThreadListAdapter', () => {
    beforeEach(() => vi.unstubAllGlobals())

    it('list maps backend threads to regular thread-list items', async () => {
        server.use(
            http.get('/agent/store', () =>
                HttpResponse.json({ threads: [{ remoteId: 'r1', title: 'First' }, { remoteId: 'r2', title: null }] }),
            ),
        )
        const result = await remoteThreadListAdapter.list()
        expect(result.threads).toEqual([
            { status: 'regular', remoteId: 'r1', title: 'First' },
            { status: 'regular', remoteId: 'r2', title: undefined },
        ])
    })

    it('initialize creates a chat and returns its ids', async () => {
        server.use(http.post('/agent/store', () => HttpResponse.json({ remoteId: 'r9', externalId: 'e9' })))
        expect(await remoteThreadListAdapter.initialize('local-1')).toEqual({ remoteId: 'r9', externalId: 'e9' })
    })

    it('rename PATCHes the new title to the chat', async () => {
        const patched: Array<{ id: string; title: string }> = []
        server.use(
            http.patch('/agent/store/:id', async ({ params, request }) => {
                const body = (await request.json()) as { title: string }
                patched.push({ id: params.id as string, title: body.title })
                return new HttpResponse(null, { status: 204 })
            }),
        )
        await remoteThreadListAdapter.rename('r1', 'Renamed')
        expect(patched).toEqual([{ id: 'r1', title: 'Renamed' }])
    })

    it('delete removes the chat', async () => {
        const deleted: string[] = []
        server.use(
            http.delete('/agent/store/:id', ({ params }) => {
                deleted.push(params.id as string)
                return new HttpResponse(null, { status: 204 })
            }),
        )
        await remoteThreadListAdapter.delete('r1')
        expect(deleted).toEqual(['r1'])
    })

    it('fetch returns a single thread as a regular item', async () => {
        server.use(http.get('/agent/store/:id', () => HttpResponse.json({ remoteId: 'r1', title: 'One' })))
        expect(await remoteThreadListAdapter.fetch('r1')).toEqual({
            status: 'regular',
            remoteId: 'r1',
            title: 'One',
        })
    })
})
