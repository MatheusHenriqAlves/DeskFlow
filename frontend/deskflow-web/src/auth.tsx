import { useCallback, useMemo, useState, type ReactNode } from 'react'
import { AuthContext } from './auth-context'
import { api } from './api'
import type { AuthResponse, User } from './types'

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(() => {
    const raw = localStorage.getItem('deskflow_user')
    return raw ? JSON.parse(raw) : null
  })

  const save = useCallback((data: AuthResponse) => {
    localStorage.setItem('deskflow_token', data.token)
    localStorage.setItem('deskflow_user', JSON.stringify(data.user))
    setUser(data.user)
  }, [])

  const login = useCallback(
    async (email: string, password: string) =>
      save(
        await api<AuthResponse>('/api/auth/login', {
          method: 'POST',
          body: JSON.stringify({ email, password }),
        }),
      ),
    [save],
  )

  const register = useCallback(
    async (name: string, email: string, password: string) =>
      save(
        await api<AuthResponse>('/api/auth/register', {
          method: 'POST',
          body: JSON.stringify({ name, email, password }),
        }),
      ),
    [save],
  )

  const logout = useCallback(() => {
    localStorage.removeItem('deskflow_token')
    localStorage.removeItem('deskflow_user')
    setUser(null)
  }, [])

  const value = useMemo(() => ({ user, login, register, logout }), [user, login, register, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
