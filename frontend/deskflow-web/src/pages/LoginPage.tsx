import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../useAuth'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()

  const [email, setEmail] = useState('user@deskflow.local')
  const [password, setPassword] = useState('User@123!')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      await login(email, password)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha no login')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="auth-page">
      <form className="auth-card" onSubmit={submit}>
        <div className="logo-large">D</div>
        <h1>Entre no DeskFlow</h1>
        <p>Gerencie chamados com prioridade inteligente.</p>

        {error && (
          <div className="error" role="alert">
            {error}
          </div>
        )}

        <label>
          E-mail
          <input value={email} onChange={e => setEmail(e.target.value)} type="email" required />
        </label>

        <label>
          Senha
          <input
            value={password}
            onChange={e => setPassword(e.target.value)}
            type="password"
            required
          />
        </label>

        <button className="primary" disabled={submitting}>
          {submitting ? 'Entrando...' : 'Entrar'}
        </button>

        <small>
          Não tem conta? <Link to="/register">Criar conta</Link>
        </small>
      </form>
    </div>
  )
}
