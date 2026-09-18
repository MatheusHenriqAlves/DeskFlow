import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../api'
import type { Ticket, TicketCategory } from '../types'

const CATEGORIES: TicketCategory[] = [
  'Hardware',
  'Software',
  'Network',
  'Access',
  'Security',
  'Other',
]

export default function NewTicketPage() {
  const navigate = useNavigate()

  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState<TicketCategory>('Other')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      const ticket = await api<Ticket>('/api/tickets', {
        method: 'POST',
        body: JSON.stringify({ title, description, category }),
      })
      navigate(`/tickets/${ticket.id}`)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao criar chamado')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="eyebrow">NOVO CHAMADO</p>
          <h1>Descreva o problema</h1>
          <p>O Smart Priority calculará a criticidade automaticamente.</p>
        </div>
      </div>

      <form className="panel form" onSubmit={submit}>
        {error && (
          <div className="error" role="alert">
            {error}
          </div>
        )}

        <label>
          Título
          <input
            value={title}
            onChange={e => setTitle(e.target.value)}
            minLength={3}
            maxLength={120}
            required
          />
        </label>

        <label>
          Categoria
          <select value={category} onChange={e => setCategory(e.target.value as TicketCategory)}>
            {CATEGORIES.map(c => (
              <option key={c}>{c}</option>
            ))}
          </select>
        </label>

        <label>
          Descrição
          <textarea
            rows={8}
            value={description}
            onChange={e => setDescription(e.target.value)}
            minLength={10}
            maxLength={2000}
            required
          />
        </label>

        <button className="primary" disabled={submitting}>
          {submitting ? 'Criando...' : 'Criar chamado'}
        </button>
      </form>
    </section>
  )
}
