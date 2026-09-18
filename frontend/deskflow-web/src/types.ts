export type UserRole = 'User' | 'Technician' | 'Admin'
export type TicketStatus = 'Open' | 'InProgress' | 'Resolved' | 'Closed'
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Critical'
export type TicketCategory = 'Hardware' | 'Software' | 'Network' | 'Access' | 'Security' | 'Other'

export interface User {
  id: string
  name: string
  email: string
  role: UserRole
  isActive: boolean
  createdAt: string
}

export interface AuthResponse {
  token: string
  expiresAt: string
  user: User
}

export interface Comment {
  id: number
  content: string
  createdAt: string
  userId: string
  userName: string
  userRole: UserRole
}

export interface History {
  id: number
  action: string
  description: string
  createdAt: string
  userId?: string
  userName?: string
}

export interface Ticket {
  id: number
  title: string
  description: string
  priority: TicketPriority
  priorityScore: number
  priorityReason: string
  status: TicketStatus
  category: TicketCategory
  createdAt: string
  updatedAt: string
  resolvedAt?: string | null
  createdByUserId: string
  createdBy: string
  assignedToUserId?: string | null
  assignedTo?: string | null
  comments: Comment[]
  history: History[]
}

export interface TicketListItem {
  id: number
  title: string
  priority: TicketPriority
  priorityScore: number
  status: TicketStatus
  category: TicketCategory
  createdAt: string
  updatedAt: string
  createdBy: string
  assignedTo?: string
}

export interface Paged<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface MetricSlice {
  name: string
  count: number
}

export interface Metrics {
  totalTickets: number
  openTickets: number
  inProgressTickets: number
  resolvedTickets: number
  closedTickets: number
  totalUsers: number
  activeTechnicians: number
  byPriority: MetricSlice[]
  byCategory: MetricSlice[]
}
