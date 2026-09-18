const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5087'

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
  ) {
    super(message)
  }
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('deskflow_token')
  const headers = new Headers(options.headers)

  if (options.body && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_URL}${path}`, { ...options, headers })

  if (!response.ok) {
    let message = `Erro ${response.status}`
    try {
      const body = await response.json()
      message = body.message ?? body.detail ?? body.title ?? message
    } catch {
      /* corpo da resposta não é JSON (ex.: erro 502 de um proxy) — mantém a mensagem padrão */
    }
    throw new ApiError(response.status, message)
  }

  if (response.status === 204) return undefined as T
  return response.json() as Promise<T>
}
