export function formatPrice(price: number): string {
    return new Intl.NumberFormat("en-NZ", {
        style: "currency",
        currency: "NZD"
    }).format(price);
}