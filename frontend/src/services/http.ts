import axios from 'axios'

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5063',
  timeout: 10_000,
  headers: { Accept: 'application/json' },
})

export function getHttpError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (!error.response) return 'Não foi possível conectar à API. Verifique se ela está em execução.'
    const data: unknown = error.response.data
    if (typeof data === 'string') return data
    if (data && typeof data === 'object' && 'title' in data && typeof data.title === 'string') return data.title
    return 'Não foi possível concluir a operação.'
  }
  return error instanceof Error ? error.message : 'Ocorreu um erro inesperado.'
}
