import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../api'
import type { Paged, TicketListItem } from '../types'

const STATUS_OPTIONS = ['Open', 'InProgress', 'Resolved', 'Closed']
const CATEGORY_OPTIONS = ['Hardware', 'Software', 'Network', 'Access', 'Security', 'Other']

export default function TicketsPage() {
  const [data, setData] = useState<Paged<TicketListItem> | null>(null)
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [category, setCategory] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(true)

  const load = () => {
    setLoading(true)
    setError('')

    const query = new URLSearchParams({ page: '1', pageSize: '50' })
    if (search) query.set('search', search)
    if (status) query.set('status', status)
    if (category) query.set('category', category)

    api<Paged<TicketListItem>>(`/api/tickets?${query}`)
      .then(setData)
      .catch(e => setError(e instanceof Error ? e.message : 'Falha ao carregar chamados.'))
      .finally(() => setLoading(false))
  }

  useEffect(() => {
    let active = true
    api<Paged<TicketListItem>>('/api/tickets?page=1&pageSize=50')
      .then(data => {
        if (active) setData(data)
      })
      .catch(e => {
        if (active) setError(e instanceof Error ? e.message : 'Falha ao carregar chamados.')
      })
      .finally(() => {
        if (active) setLoading(false)
      })
    return () => {
      active = false
    }
  }, [])

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="eyebrow">CHAMADOS</p>
          <h1>Fila de atendimento</h1>
        </div>
        <Link className="primary button-link" to="/tickets/new">
          + Novo chamado
        </Link>
      </div>

      <div className="filters">
        <input
          placeholder="Buscar chamado..."
          value={search}
          onChange={e => setSearch(e.target.value)}
        />

        <select value={status} onChange={e => setStatus(e.target.value)}>
          <option value="">Todos os status</option>
          {STATUS_OPTIONS.map(s => (
            <option key={s}>{s}</option>
          ))}
        </select>

        <select value={category} onChange={e => setCategory(e.target.value)}>
          <option value="">Todas categorias</option>
          {CATEGORY_OPTIONS.map(c => (
            <option key={c}>{c}</option>
          ))}
        </select>

        <button onClick={load} disabled={loading}>
          {loading ? 'Filtrando...' : 'Filtrar'}
        </button>
      </div>

      {error && (
        <div className="error" role="alert">
          {error}
        </div>
      )}

      <div className="panel">
        {data?.items.map(t => (
          <Link className="ticket-row" to={`/tickets/${t.id}`} key={t.id}>
            <div>
              <strong>
                #{t.id} {t.title}
              </strong>
              <small>
                {t.createdBy} · {new Date(t.createdAt).toLocaleString()}
              </small>
            </div>

            <div className="badges">
              <span className={`badge priority-${t.priority.toLowerCase()}`}>{t.priority}</span>
              <span className="badge">{t.category}</span>
              <span className="badge">{t.status}</span>
            </div>
          </Link>
        ))}

        {!loading && data?.items.length === 0 && (
          <div className="empty">Nenhum chamado encontrado.</div>
        )}
      </div>
    </section>
  )
}
