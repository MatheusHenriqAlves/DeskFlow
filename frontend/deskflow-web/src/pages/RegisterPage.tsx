import { useState, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../useAuth'

export default function RegisterPage() {
  const { register } = useAuth()
  const navigate = useNavigate()

  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setSubmitting(true)
    try {
      await register(name, email, password)
      navigate('/')
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha no cadastro')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="auth-page">
      <form className="auth-card" onSubmit={submit}>
        <div className="logo-large">D</div>
        <h1>Crie sua conta</h1>
        <p>Abra e acompanhe chamados em poucos minutos.</p>

        {error && (
          <div className="error" role="alert">
            {error}
          </div>
        )}

        <label>
          Nome
          <input value={name} onChange={e => setName(e.target.value)} required minLength={2} />
        </label>

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
            minLength={8}
          />
        </label>

        <button className="primary" disabled={submitting}>
          {submitting ? 'Cadastrando...' : 'Cadastrar'}
        </button>

        <small>
          Já tem conta? <Link to="/login">Entrar</Link>
        </small>
      </form>
    </div>
  )
}
