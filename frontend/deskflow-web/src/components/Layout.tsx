import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../useAuth'

export default function Layout() {
  const { user, logout } = useAuth()

  return (
    <div className="shell">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">D</span>
          <div>
            <strong>DeskFlow</strong>
            <small>Service Desk</small>
          </div>
        </div>

        <nav>
          <NavLink to="/">Visão geral</NavLink>
          <NavLink to="/tickets">Chamados</NavLink>
          <NavLink to="/tickets/new">Novo chamado</NavLink>
          {user?.role === 'Admin' && <NavLink to="/admin/users">Usuários</NavLink>}
        </nav>

        <div className="sidebar-user">
          <span>{user?.name}</span>
          <small>{user?.role}</small>
          <button className="link-button" onClick={logout}>
            Sair
          </button>
        </div>
      </aside>

      <main className="content">
        <Outlet />
      </main>
    </div>
  )
}
