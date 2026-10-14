// All kommunikation med backenden samlad på ett ställe.
import type {
  AuthResponse,
  Contact,
  Customer,
  Deal,
  DealStage,
  SaveContact,
  SaveCustomer,
  SaveDeal,
} from './types'

const TOKEN_KEY = 'crm_token'

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token: string | null) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token)
  } else {
    localStorage.removeItem(TOKEN_KEY)
  }
}

/**
 * Gemensam hjälpfunktion för alla anrop.
 * Lägger till JSON-headers och token, och kastar ett fel om svaret inte är OK.
 */
async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  const token = getToken()
  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  const response = await fetch(`/api${path}`, { ...options, headers })

  if (response.status === 401 && token) {
    // Token har gått ut eller är ogiltig: logga ut och ladda om sidan.
    setToken(null)
    window.location.href = '/login'
  }

  if (!response.ok) {
    throw new Error(await readError(response))
  }

  // 204 No Content har ingen body.
  if (response.status === 204) {
    return undefined as T
  }
  return (await response.json()) as T
}

/** Försöker plocka fram ett läsbart felmeddelande från backenden. */
async function readError(response: Response): Promise<string> {
  const text = await response.text()
  try {
    const data = JSON.parse(text)
    // Valideringsfel från ASP.NET Core ser ut så här: { errors: { Name: ["..."] } }
    if (data.errors) {
      return Object.values(data.errors).flat().join(' ')
    }
    return data.title ?? text
  } catch {
    return text || `Något gick fel (${response.status})`
  }
}

/** Bygger en query string och hoppar över tomma värden. */
function query(params: Record<string, string | number | undefined>): string {
  const search = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== '') {
      search.set(key, String(value))
    }
  }
  const result = search.toString()
  return result ? `?${result}` : ''
}

// ---------- Inloggning ----------

export const login = (email: string, password: string) =>
  request<AuthResponse>('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) })

export const register = (email: string, password: string) =>
  request<AuthResponse>('/auth/register', { method: 'POST', body: JSON.stringify({ email, password }) })

// ---------- Kunder ----------

export const getCustomers = (search?: string) => request<Customer[]>(`/customers${query({ search })}`)

export const createCustomer = (data: SaveCustomer) =>
  request<Customer>('/customers', { method: 'POST', body: JSON.stringify(data) })

export const updateCustomer = (id: number, data: SaveCustomer) =>
  request<void>(`/customers/${id}`, { method: 'PUT', body: JSON.stringify(data) })

export const deleteCustomer = (id: number) => request<void>(`/customers/${id}`, { method: 'DELETE' })

// ---------- Kontakter ----------

export const getContacts = (search?: string) => request<Contact[]>(`/contacts${query({ search })}`)

export const createContact = (data: SaveContact) =>
  request<Contact>('/contacts', { method: 'POST', body: JSON.stringify(data) })

export const updateContact = (id: number, data: SaveContact) =>
  request<void>(`/contacts/${id}`, { method: 'PUT', body: JSON.stringify(data) })

export const deleteContact = (id: number) => request<void>(`/contacts/${id}`, { method: 'DELETE' })

// ---------- Affärer ----------

export const getDeals = (search?: string, stage?: DealStage | '') =>
  request<Deal[]>(`/deals${query({ search, stage })}`)

export const createDeal = (data: SaveDeal) =>
  request<Deal>('/deals', { method: 'POST', body: JSON.stringify(data) })

export const updateDeal = (id: number, data: SaveDeal) =>
  request<void>(`/deals/${id}`, { method: 'PUT', body: JSON.stringify(data) })

export const deleteDeal = (id: number) => request<void>(`/deals/${id}`, { method: 'DELETE' })
