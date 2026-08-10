/**
 * Save text to the user's device as a file download.
 */
export function downloadTextFile(filename: string, text: string, type = 'text/plain') {
    const url = URL.createObjectURL(new Blob([text], { type }));
    const link = document.createElement('a');

    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Deferred: revoking in the same tick can cancel the download before it starts.
    setTimeout(() => URL.revokeObjectURL(url), 0);
}
