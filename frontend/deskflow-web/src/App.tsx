import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AuthProvider } from './auth'
import { useAuth } from './useAuth'
import Layout from './components/Layout'
import { AdminRoute, ProtectedRoute } from './components/ProtectedRoute'
import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import DashboardPage from './pages/DashboardPage'
import TicketsPage from './pages/TicketsPage'
import NewTicketPage from './pages/NewTicketPage'
import TicketDetailsPage from './pages/TicketDetailsPage'
import AdminUsersPage from './pages/AdminUsersPage'

function RoutesTree() {
  const { user } = useAuth()

  return (
    <Routes>
      <Route path="/login" element={user ? <Navigate to="/" /> : <LoginPage />} />
      <Route path="/register" element={user ? <Navigate to="/" /> : <RegisterPage />} />

      <Route
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="tickets" element={<TicketsPage />} />
        <Route path="tickets/new" element={<NewTicketPage />} />
        <Route path="tickets/:id" element={<TicketDetailsPage />} />
        <Route
          path="admin/users"
          element={
            <AdminRoute>
              <AdminUsersPage />
            </AdminRoute>
          }
        />
      </Route>

      <Route path="*" element={<Navigate to="/" />} />
    </Routes>
  )
}

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <RoutesTree />
      </AuthProvider>
    </BrowserRouter>
  )
}
