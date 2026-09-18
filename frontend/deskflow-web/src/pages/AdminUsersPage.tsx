import { useEffect, useState } from 'react'
import { api } from '../api'
import { useAuth } from '../useAuth'
import type { User, UserRole } from '../types'

const ROLE_OPTIONS: UserRole[] = ['User', 'Technician', 'Admin']

export default function AdminUsersPage() {
  const { user: currentUser } = useAuth()

  const [users, setUsers] = useState<User[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [pendingUserId, setPendingUserId] = useState<string | null>(null)

  const load = () =>
    api<User[]>('/api/users')
      .then(setUsers)
      .catch(e => setError(e instanceof Error ? e.message : 'Falha ao carregar usuários.'))
      .finally(() => setLoading(false))

  useEffect(() => {
    void load()
  }, [])

  const changeRole = async (id: string, role: UserRole) => {
    setError('')
    setPendingUserId(id)
    try {
      await api(`/api/users/${id}/role`, {
        method: 'PUT',
        body: JSON.stringify({ role }),
      })
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Não foi possível alterar o perfil.')
    } finally {
      setPendingUserId(null)
    }
  }

  const toggleActive = async (target: User) => {
    setError('')
    setPendingUserId(target.id)
    try {
      await api(`/api/users/${target.id}/active`, {
        method: 'PUT',
        body: JSON.stringify({ isActive: !target.isActive }),
      })
      await load()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Não foi possível alterar o status do usuário.')
    } finally {
      setPendingUserId(null)
    }
  }

  return (
    <section>
      <div className="page-heading">
        <div>
          <p className="eyebrow">ADMINISTRAÇÃO</p>
          <h1>Usuários</h1>
        </div>
      </div>

      {error && (
        <div className="error" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <div>Carregando usuários...</div>
      ) : (
        <div className="panel table-wrap">
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>E-mail</th>
                <th>Perfil</th>
                <th>Status</th>
                <th>Ação</th>
              </tr>
            </thead>

            <tbody>
              {users.map(u => {
                const isSelf = u.id === currentUser?.id
                const busy = pendingUserId === u.id

                return (
                  <tr key={u.id}>
                    <td>
                      {u.name}
                      {isSelf && <small> (você)</small>}
                    </td>
                    <td>{u.email}</td>
                    <td>
                      <select
                        value={u.role}
                        onChange={e => changeRole(u.id, e.target.value as UserRole)}
                        // Um admin não pode rebaixar a si mesmo — evita ficar sem
                        // nenhum administrador ativo no sistema. O backend também
                        // bloqueia isso; aqui só evitamos que a pessoa tente e
                        // tome um erro sem necessidade.
                        disabled={busy || (isSelf && u.role === 'Admin')}
                        title={
                          isSelf && u.role === 'Admin'
                            ? 'Você não pode remover sua própria permissão de administrador.'
                            : undefined
                        }
                      >
                        {ROLE_OPTIONS.map(r => (
                          <option key={r}>{r}</option>
                        ))}
                      </select>
                    </td>
                    <td>{u.isActive ? 'Ativo' : 'Inativo'}</td>
                    <td>
                      <button
                        onClick={() => toggleActive(u)}
                        disabled={busy || (isSelf && u.isActive)}
                        title={
                          isSelf && u.isActive
                            ? 'Você não pode desativar a sua própria conta.'
                            : undefined
                        }
                      >
                        {busy ? 'Aguarde...' : u.isActive ? 'Desativar' : 'Ativar'}
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
