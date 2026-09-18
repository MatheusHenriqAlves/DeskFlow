import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { api } from './api'

describe('api()', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('throws an ApiError carrying the status and message returned by the server', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 401,
        json: () => Promise.resolve({ message: 'E-mail ou senha inválidos.' }),
      }),
    )

    await expect(api('/api/auth/login', { method: 'POST' })).rejects.toMatchObject({
      status: 401,
      message: 'E-mail ou senha inválidos.',
    })
  })

  it('falls back to a generic message when the error body is not JSON', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        status: 502,
        json: () => Promise.reject(new Error('not json')),
      }),
    )

    await expect(api('/api/tickets')).rejects.toMatchObject({ status: 502, message: 'Erro 502' })
  })

  it('sends the stored bearer token in the Authorization header', async () => {
    localStorage.setItem('deskflow_token', 'abc123')
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: () => Promise.resolve({ ok: true }),
    })
    vi.stubGlobal('fetch', fetchMock)

    await api('/api/tickets')

    const [, options] = fetchMock.mock.calls[0] as [string, RequestInit]
    const headers = options.headers as Headers
    expect(headers.get('Authorization')).toBe('Bearer abc123')
  })

  it('does not attach an Authorization header when there is no stored token', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      status: 200,
      json: () => Promise.resolve({ ok: true }),
    })
    vi.stubGlobal('fetch', fetchMock)

    await api('/api/health')

    const [, options] = fetchMock.mock.calls[0] as [string, RequestInit]
    const headers = options.headers as Headers
    expect(headers.has('Authorization')).toBe(false)
  })

  it('returns undefined for 204 No Content responses instead of trying to parse a body', async () => {
    vi.stubGlobal('fetch', vi.fn().mockResolvedValue({ ok: true, status: 204 }))

    await expect(api('/api/tickets/1')).resolves.toBeUndefined()
  })
})
