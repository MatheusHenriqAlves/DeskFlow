import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { act, fireEvent, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { AuthProvider } from '../auth'
import { api, ApiError } from '../api'
import type { Ticket, UserRole } from '../types'
import TicketDetailsPage from './TicketDetailsPage'

vi.mock('../api', async () => {
  const actual = await vi.importActual<typeof import('../api')>('../api')
  return { ...actual, api: vi.fn() }
})

const ticket: Ticket = {
  id: 8,
  title: 'Sistema de segurança caiu',
  description: 'Estamos sofrendo ataque hacker',
  priority: 'Critical',
  priorityScore: 14,
  priorityReason: 'Incidente grave de segurança',
  status: 'Open',
  category: 'Security',
  createdAt: '2026-09-17T12:00:00Z',
  updatedAt: '2026-09-17T12:00:00Z',
  createdByUserId: 'user-1',
  createdBy: 'Usuário',
  comments: [],
  history: [],
}

function renderTicket(role: UserRole = 'Admin') {
  localStorage.setItem('deskflow_user', JSON.stringify({ id: 'user-1', name: 'Teste', role }))
  return render(
    <MemoryRouter initialEntries={['/tickets/8']}>
      <AuthProvider>
        <Routes>
          <Route path="/tickets/:id" element={<TicketDetailsPage />} />
          <Route path="/tickets" element={<p>Lista de chamados</p>} />
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  )
}

describe('TicketDetailsPage', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.mocked(api).mockReset().mockResolvedValueOnce(ticket)
  })

  afterEach(() => vi.restoreAllMocks())

  it.each(['User', 'Technician'] as const)('does not show deletion to %s', async role => {
    renderTicket(role)
    await screen.findByRole('heading', { name: ticket.title })
    expect(screen.queryByRole('button', { name: /excluir chamado/i })).not.toBeInTheDocument()
  })

  it('keeps the ticket when the admin cancels confirmation', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false)
    renderTicket()
    await userEvent.click(await screen.findByRole('button', { name: /excluir chamado/i }))
    expect(api).toHaveBeenCalledTimes(1)
    expect(screen.getByRole('heading', { name: ticket.title })).toBeInTheDocument()
  })

  it('deletes after confirmation and returns to the list', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true)
    vi.mocked(api).mockResolvedValueOnce(undefined)
    renderTicket()
    await userEvent.click(await screen.findByRole('button', { name: /excluir chamado/i }))
    expect(await screen.findByText('Lista de chamados')).toBeInTheDocument()
    expect(api).toHaveBeenLastCalledWith('/api/tickets/8', { method: 'DELETE' })
  })

  it('shows a failed action without removing the ticket details', async () => {
    vi.mocked(api).mockRejectedValueOnce(new ApiError(409, 'Não foi possível assumir.'))
    renderTicket()
    await userEvent.click(await screen.findByRole('button', { name: /assumir chamado/i }))
    expect(await screen.findByRole('alert')).toHaveTextContent('Não foi possível assumir.')
    expect(screen.getByRole('heading', { name: ticket.title })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: /assumir chamado/i })).toBeEnabled()
  })

  it('prevents duplicate actions while the first request is pending', async () => {
    let resolveAction!: (value: unknown) => void
    vi.mocked(api).mockReturnValueOnce(
      new Promise(resolve => {
        resolveAction = resolve
      }),
    )
    vi.mocked(api).mockResolvedValueOnce(ticket)
    renderTicket()
    const button = await screen.findByRole('button', { name: /assumir chamado/i })
    fireEvent.click(button)
    fireEvent.click(button)
    expect(button).toBeDisabled()
    expect(api).toHaveBeenCalledTimes(2)
    await act(async () => {
      resolveAction(undefined)
    })
    expect(button).toBeEnabled()
  })
})
