import axios from 'axios';
import { toast } from '@/components/CustomToast';

// Maps HTTP status codes to user-facing fallback messages for a request
type StatusMessages = Record<number, string>;

/**
 * Helper to set an appropriate message for toast
 * Rethrows the error so a value-returning function can call it in a catch block
 * and abort the success path
 */
export function handleApiError(
    error: unknown,
    statusMessages: StatusMessages = {},
    title = 'Something went wrong',
): never {
    let message: string | undefined;

    if (axios.isAxiosError(error)) {
        const data = error.response?.data;
        const status = error.response?.status;
        const text = typeof data === 'string' ? data : null;
        // use message from server if possible, otherwise fallback to messages from ts
        message = text ?? (status != null ? statusMessages[status] : undefined);
    }

    toast.error(title, message);
    throw error;
}
