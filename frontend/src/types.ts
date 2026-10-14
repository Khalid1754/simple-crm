// TypeScript-typer som matchar DTO:erna i backenden (SimpleCrm.Api/Dtos).

export interface Customer {
  id: number
  name: string
  orgNumber: string | null
  email: string | null
  phone: string | null
  city: string | null
  createdAt: string
  contactCount: number
  dealCount: number
}

export interface SaveCustomer {
  name: string
  orgNumber: string | null
  email: string | null
  phone: string | null
  city: string | null
}

export interface Contact {
  id: number
  firstName: string
  lastName: string
  email: string | null
  phone: string | null
  jobTitle: string | null
  customerId: number
  customerName: string
}

export interface SaveContact {
  firstName: string
  lastName: string
  email: string | null
  phone: string | null
  jobTitle: string | null
  customerId: number
}

// Måste matcha enumen DealStage i backenden.
export const dealStages = ['Lead', 'Contacted', 'Proposal', 'Won', 'Lost'] as const
export type DealStage = (typeof dealStages)[number]

// Svenska namn att visa i gränssnittet.
export const dealStageLabels: Record<DealStage, string> = {
  Lead: 'Lead',
  Contacted: 'Kontaktad',
  Proposal: 'Offert',
  Won: 'Vunnen',
  Lost: 'Förlorad',
}

export interface Deal {
  id: number
  title: string
  amount: number
  stage: DealStage
  expectedCloseDate: string | null
  createdAt: string
  customerId: number
  customerName: string
}

export interface SaveDeal {
  title: string
  amount: number
  stage: DealStage
  expectedCloseDate: string | null
  customerId: number
}

export interface AuthResponse {
  token: string
  email: string
  expiresAt: string
}
