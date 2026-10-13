import { lazy, Suspense } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom'
import { AppLayout } from '@/components/layout/AppLayout'
const HomePage = lazy(() => import('@/routes/HomePage').then(module => ({ default: module.HomePage })))

const queryClient = new QueryClient({ defaultOptions: {
  queries: { staleTime: 30_000, retry: 1, refetchOnWindowFocus: false },
  mutations: { retry: false },
} })

export function App() {
  return <QueryClientProvider client={queryClient}>
    <BrowserRouter><AppLayout><Suspense fallback={<p role="status">Carregando...</p>}><Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="*" element={<div><h1 className="text-2xl font-semibold">Página não encontrada</h1><Link to="/" className="mt-4 inline-block text-primary">Voltar ao início</Link></div>} />
    </Routes></Suspense></AppLayout></BrowserRouter>
  </QueryClientProvider>
}
