import { beforeEach, describe, expect, it, vi } from 'vitest'
import { act, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { AuthProvider } from '../auth'
import LoginPage from './LoginPage'
import { ApiError } from '../api'

// Substitui o módulo real de chamadas HTTP por um mock: os testes de UI não
// devem depender de uma API rodando de verdade. `ApiError` é reaproveitada
// da implementação real para os testes ficarem fiéis ao comportamento real
// de erro (ver src/api.test.ts para os testes da própria função `api`).
vi.mock('../api', async () => {
  const actual = await vi.importActual<typeof import('../api')>('../api')
  return { ...actual, api: vi.fn() }
})

import { api } from '../api'

function renderLoginPage() {
  return render(
    <MemoryRouter>
      <AuthProvider>
        <LoginPage />
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('LoginPage', () => {
  beforeEach(() => {
    vi.mocked(api).mockReset()
    localStorage.clear()
  })

  it('shows the error message returned by the API when login fails', async () => {
    vi.mocked(api).mockRejectedValueOnce(new ApiError(401, 'E-mail ou senha inválidos.'))
    const user = userEvent.setup()
    renderLoginPage()

    await user.click(screen.getByRole('button', { name: /entrar/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent('E-mail ou senha inválidos.')
  })

  it('disables the submit button while the login request is pending', async () => {
    let resolveLogin: (value: unknown) => void = () => {}
    vi.mocked(api).mockReturnValueOnce(
      new Promise(resolve => {
        resolveLogin = resolve
      }),
    )
    const user = userEvent.setup()
    renderLoginPage()

    const button = screen.getByRole('button', { name: /entrar/i })
    await user.click(button)

    expect(button).toBeDisabled()

    await act(async () => {
      resolveLogin({
        token: 'fake-token',
        expiresAt: new Date().toISOString(),
        user: {
          id: '1',
          name: 'Teste',
          email: 'user@deskflow.local',
          role: 'User',
          isActive: true,
          createdAt: '',
        },
      })
    })
    expect(button).toBeEnabled()
  })
})
