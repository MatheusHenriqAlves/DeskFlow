import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import { useAuth } from '../useAuth'
import type { Metrics, Paged, TicketListItem, User } from '../types'

export default function DashboardPage() {
  const { user } = useAuth()
  return <DashboardContent key={`${user?.id}:${user?.role}`} user={user} />
}

function DashboardContent({ user }: { user: User | null }) {
  const [tickets, setTickets] = useState<Paged<TicketListItem> | null>(null)
  const [metrics, setMetrics] = useState<Metrics | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true

    const requests: Promise<unknown>[] = [
      api<Paged<TicketListItem>>('/api/tickets?pageSize=5').then(data => {
        if (active) setTickets(data)
      }),
    ]
    if (user?.role === 'Admin') {
      requests.push(
        api<Metrics>('/api/metrics').then(data => {
          if (active) setMetrics(data)
        }),
      )
    }

    Promise.all(requests)
      .catch(e => {
        if (active) setError(e instanceof Error ? e.message : 'Falha ao carregar o painel.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [user?.role])

  const open = tickets?.items.filter(x => x.status === 'Open').length ?? 0

  if (error) {
    return (
      <div className="error" role="alert">
        {error}
      </div>
    )
  }

  if (loading) {
    return <div>Carregando painel...</div>
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="eyebrow">CENTRAL DE OPERAÇÕES</p>
          <h1>Olá, {user?.name}</h1>
          <p>Acompanhe o que precisa de atenção agora.</p>
        </div>
      </div>

      <div className="stats">
        <div className="stat">
          <small>Chamados visíveis</small>
          <strong>{metrics?.totalTickets ?? tickets?.totalItems ?? 0}</strong>
        </div>

        <div className="stat">
          <small>Abertos</small>
          <strong>{metrics?.openTickets ?? open}</strong>
        </div>

        <div className="stat">
          <small>Em andamento</small>
          <strong>{metrics?.inProgressTickets ?? '—'}</strong>
        </div>

        <div className="stat">
          <small>Resolvidos</small>
          <strong>{metrics?.resolvedTickets ?? '—'}</strong>
        </div>
      </div>

      <div className="panel">
        <div className="panel-title">
          <h2>Chamados recentes</h2>
        </div>

        {tickets?.items.map(t => (
          <Link className="ticket-row" to={`/tickets/${t.id}`} key={t.id}>
            <div>
              <strong>
                #{t.id} {t.title}
              </strong>
              <small>
                {t.category} · {t.createdBy}
              </small>
            </div>

            <div className="badges">
              <span className={`badge priority-${t.priority.toLowerCase()}`}>{t.priority}</span>
              <span className="badge">{t.status}</span>
            </div>
          </Link>
        ))}

        {tickets?.items.length === 0 && <div className="empty">Nenhum chamado por aqui ainda.</div>}
      </div>
    </section>
  )
}
