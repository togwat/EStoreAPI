// takes continuous string of digits and formats it into phone no. with spaces
// e.g. 0221234567 -> 022 123 4567
export function formatPhone(phone?: string): string | null {
    if (!phone) return null;

  const digits = phone.replace(/\D/g, "")

    // +64 international prefix -> replace with leading 0
    const normalized = digits.startsWith("64") && digits.length > 9
        ? "0" + digits.slice(2)
        : digits;

    // Mobile: 021, 022, 027, 028, 029 -> 3 + 3 + 4
    if (/^02\d/.test(normalized)) {
        return normalized.replace(/^(0\d{2})(\d{3})(\d{1,4})$/, "$1 $2 $3");
    }

    // Landline: 09, 04, 03, 07, 06, etc. -> 2 + 3 + 4
    if (/^0[3-9]/.test(normalized)) {
        return normalized.replace(/^(0\d)(\d{3})(\d{1,4})$/, "$1 $2 $3");
    }

    // 0800 / 0900 freephone -> 4 + 3 + 3
    if (/^0[89]00/.test(normalized)) {
        return normalized.replace(/^(0[89]00)(\d{3})(\d{1,3})$/, "$1 $2 $3");
    }

    return phone;   // unrecognised, return as-is
}