import { beforeEach, describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { AuthProvider } from '../auth'
import AdminUsersPage from './AdminUsersPage'
import type { User } from '../types'

vi.mock('../api', async () => {
  const actual = await vi.importActual<typeof import('../api')>('../api')
  return { ...actual, api: vi.fn() }
})

import { api } from '../api'

const ADMIN: User = {
  id: 'admin-1',
  name: 'Admin Logado',
  email: 'admin@deskflow.local',
  role: 'Admin',
  isActive: true,
  createdAt: '',
}

const OTHER_USER: User = {
  id: 'user-2',
  name: 'Outro Usuário',
  email: 'user2@deskflow.local',
  role: 'User',
  isActive: true,
  createdAt: '',
}

function renderPage() {
  // Simula uma sessão de admin já autenticada, sem passar pela tela de login.
  localStorage.setItem('deskflow_token', 'fake-token')
  localStorage.setItem('deskflow_user', JSON.stringify(ADMIN))

  return render(
    <MemoryRouter>
      <AuthProvider>
        <AdminUsersPage />
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('AdminUsersPage', () => {
  beforeEach(() => {
    vi.mocked(api).mockReset()
    localStorage.clear()
  })

  it('disables self-demotion and self-deactivation controls for the logged-in admin', async () => {
    vi.mocked(api).mockResolvedValueOnce([ADMIN, OTHER_USER])
    renderPage()

    const adminRow = (await screen.findByText('Admin Logado')).closest('tr')!
    const otherRow = screen.getByText('Outro Usuário').closest('tr')!

    // A própria conta do admin logado deve vir com o seletor de perfil e o
    // botão de desativar bloqueados — essa é a mesma regra aplicada no backend
    // (ver UserService.UpdateRoleAsync / SetActiveAsync).
    expect(within(adminRow).getByRole('combobox')).toBeDisabled()
    expect(within(adminRow).getByRole('button', { name: /desativar/i })).toBeDisabled()

    // Para qualquer outro usuário, as ações continuam liberadas normalmente.
    expect(within(otherRow).getByRole('combobox')).toBeEnabled()
    expect(within(otherRow).getByRole('button', { name: /desativar/i })).toBeEnabled()
  })
})

// Pequeno helper local para consultas escopadas a um elemento (equivalente ao
// `within` de @testing-library/dom, reexportado por @testing-library/react).
import { within } from '@testing-library/react'
