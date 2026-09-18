import { useEffect, useRef, useState, type FormEvent } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../useAuth'
import type { Ticket, TicketCategory, TicketStatus } from '../types'

const STATUS_OPTIONS: TicketStatus[] = ['Open', 'InProgress', 'Resolved', 'Closed']
const CATEGORY_OPTIONS: TicketCategory[] = [
  'Hardware',
  'Software',
  'Network',
  'Access',
  'Security',
  'Other',
]

export default function TicketDetailsPage() {
  const { id } = useParams()
  return <TicketDetails key={id} id={id} />
}

function TicketDetails({ id }: { id: string | undefined }) {
  const navigate = useNavigate()
  const { user } = useAuth()

  const [ticket, setTicket] = useState<Ticket | null>(null)
  const [comment, setComment] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  // Erros de ações (assumir, mudar status/categoria, comentar, excluir) ficam
  // separados do erro de carregamento inicial, e "actionPending" evita que o
  // usuário clique duas vezes enquanto uma requisição ainda está em andamento.
  const [actionError, setActionError] = useState('')
  const [actionPending, setActionPending] = useState(false)
  const actionLock = useRef(false)

  const load = async () => {
    const updated = await api<Ticket>(`/api/tickets/${id}`)
    setTicket(updated)
  }

  useEffect(() => {
    let active = true
    api<Ticket>(`/api/tickets/${id}`)
      .then(data => {
        if (active) setTicket(data)
      })
      .catch(e => {
        if (active) setError(e instanceof Error ? e.message : 'Falha ao carregar o chamado.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [id])

  const runAction = async (action: () => Promise<unknown>) => {
    if (actionLock.current) return
    actionLock.current = true
    setActionError('')
    setActionPending(true)
    try {
      await action()
      await load()
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Não foi possível concluir a ação.')
    } finally {
      actionLock.current = false
      setActionPending(false)
    }
  }

  const addComment = (e: FormEvent) => {
    e.preventDefault()
    if (!comment.trim()) return

    void runAction(async () => {
      await api(`/api/tickets/${id}/comments`, {
        method: 'POST',
        body: JSON.stringify({ content: comment }),
      })
      setComment('')
    })
  }

  const assign = () => runAction(() => api(`/api/tickets/${id}/assign`, { method: 'POST' }))

  const changeStatus = (status: TicketStatus) =>
    runAction(() =>
      api(`/api/tickets/${id}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status }),
      }),
    )

  const changeCategory = (category: TicketCategory) =>
    runAction(() =>
      api(`/api/tickets/${id}/category`, {
        method: 'PUT',
        body: JSON.stringify({ category }),
      }),
    )

  const deleteTicket = async () => {
    if (!ticket || user?.role !== 'Admin' || actionLock.current) return
    if (
      !window.confirm(
        `Excluir o chamado #${ticket.id} — ${ticket.title}? Esta ação não pode ser desfeita.`,
      )
    )
      return

    actionLock.current = true
    setActionError('')
    setActionPending(true)
    try {
      await api<void>(`/api/tickets/${ticket.id}`, { method: 'DELETE' })
      navigate('/tickets', { replace: true })
    } catch (e) {
      setActionError(e instanceof Error ? e.message : 'Não foi possível excluir o chamado.')
      actionLock.current = false
      setActionPending(false)
    }
  }

  if (error) {
    return (
      <div className="error" role="alert">
        {error}
      </div>
    )
  }

  if (loading || !ticket) {
    return <div>Carregando...</div>
  }

  const staff = user?.role === 'Technician' || user?.role === 'Admin'

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="eyebrow">CHAMADO #{ticket.id}</p>
          <h1>{ticket.title}</h1>
          <p>
            Aberto por {ticket.createdBy} · {new Date(ticket.createdAt).toLocaleString()}
          </p>
        </div>

        <div className="badges">
          <span className={`badge priority-${ticket.priority.toLowerCase()}`}>
            {ticket.priority} · score {ticket.priorityScore}
          </span>
          <span className="badge">{ticket.status}</span>
        </div>
      </div>

      {actionError && (
        <div className="error" role="alert">
          {actionError}
        </div>
      )}

      <div className="details-grid">
        <div>
          <div className="panel">
            <h2>Descrição</h2>
            <p className="description">{ticket.description}</p>

            <div className="meta">
              <div>
                <small>Categoria</small>
                <strong>{ticket.category}</strong>
              </div>

              <div>
                <small>Responsável</small>
                <strong>{ticket.assignedTo ?? 'Não atribuído'}</strong>
              </div>
            </div>

            <div className="smart">
              <strong>Smart Priority</strong>
              <p>{ticket.priorityReason}</p>
            </div>
          </div>

          {staff && (
            <div className="panel actions">
              <h2>Ações técnicas</h2>

              <button onClick={assign} disabled={actionPending}>
                Assumir chamado
              </button>

              <select
                value={ticket.status}
                onChange={e => changeStatus(e.target.value as TicketStatus)}
                disabled={actionPending}
              >
                {STATUS_OPTIONS.map(s => (
                  <option key={s}>{s}</option>
                ))}
              </select>

              <select
                value={ticket.category}
                onChange={e => changeCategory(e.target.value as TicketCategory)}
                disabled={actionPending}
              >
                {CATEGORY_OPTIONS.map(c => (
                  <option key={c}>{c}</option>
                ))}
              </select>
            </div>
          )}

          {user?.role === 'Admin' && (
            <div className="panel">
              <h2>Administração do chamado</h2>
              <button
                type="button"
                disabled={actionPending}
                onClick={deleteTicket}
                className="delete-ticket-button"
              >
                {actionPending ? 'Aguarde...' : 'Excluir chamado'}
              </button>
            </div>
          )}

          <div className="panel">
            <h2>Comentários</h2>

            {ticket.comments.map(c => (
              <div className="comment" key={c.id}>
                <strong>{c.userName}</strong>
                <small>{new Date(c.createdAt).toLocaleString()}</small>
                <p>{c.content}</p>
              </div>
            ))}

            <form className="comment-form" onSubmit={addComment}>
              <textarea
                placeholder="Adicionar comentário..."
                value={comment}
                onChange={e => setComment(e.target.value)}
                required
              />
              <button className="primary" type="submit" disabled={actionPending}>
                {actionPending ? 'Enviando...' : 'Comentar'}
              </button>
            </form>
          </div>
        </div>

        <div className="panel">
          <h2>Histórico</h2>

          <div className="timeline">
            {ticket.history.map(h => (
              <div className="timeline-item" key={h.id}>
                <span></span>
                <div>
                  <strong>{h.action}</strong>
                  <p>{h.description}</p>
                  <small>
                    {h.userName ?? 'Sistema'} · {new Date(h.createdAt).toLocaleString()}
                  </small>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  )
}
