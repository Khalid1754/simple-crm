// Små hjälpfunktioner som används på flera sidor.

/** Gör om en tom textruta till null, så att backenden får null i stället för "". */
export function emptyToNull(value: string): string | null {
  const trimmed = value.trim()
  return trimmed === '' ? null : trimmed
}

/** Formaterar ett belopp som svenska kronor, t.ex. "250 000 kr". */
export function formatSek(amount: number): string {
  return amount.toLocaleString('sv-SE', { style: 'currency', currency: 'SEK', maximumFractionDigits: 0 })
}
