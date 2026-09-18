import { Navigate } from 'react-router-dom'
import { useAuth } from '../useAuth'
import type { ReactNode } from 'react'

export function ProtectedRoute({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  return user ? children : <Navigate to="/login" replace />
}
export function AdminRoute({ children }: { children: ReactNode }) {
  const { user } = useAuth()
  return user?.role === 'Admin' ? children : <Navigate to="/" replace />
}
