import { useState, type ReactNode } from 'react'
import { NavLink } from 'react-router-dom'
import { Moon, Sun, Trophy } from 'lucide-react'
import { Button } from '@/components/ui/button'

export function AppLayout({ children }: { children: ReactNode }) {
  const [dark, setDark] = useState(() => {
    const saved = localStorage.getItem('mini-brasfoot-theme')
    return saved !== 'light'
  })
  function toggleTheme() {
    const next = !dark
    document.documentElement.classList.toggle('dark', next)
    localStorage.setItem('mini-brasfoot-theme', next ? 'dark' : 'light')
    setDark(next)
  }
  return <div className="min-h-screen">
    <header className="border-b bg-card">
      <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-5 py-5">
        <NavLink to="/" className="flex items-center gap-3 font-semibold tracking-tight">
          <span className="rounded-lg bg-primary p-2 text-primary-foreground"><Trophy aria-hidden="true" size={22} /></span>
          <span>MINI BRASFOOT<span className="block text-xs font-normal text-muted-foreground">Seu clube. Sua história.</span></span>
        </NavLink>
        <Button variant="outline" size="icon" onClick={toggleTheme} aria-label={dark ? 'Ativar tema claro' : 'Ativar tema escuro'}>
          {dark ? <Sun /> : <Moon />}
        </Button>
      </div>
    </header>
    <main className="mx-auto max-w-6xl px-5 py-10">{children}</main>
    <footer className="mx-auto max-w-6xl px-5 pb-8 text-xs text-muted-foreground">Mini Brasfoot · Uma nova temporada começa aqui.</footer>
  </div>
}
