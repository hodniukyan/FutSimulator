import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { App } from '@/app/App'
import '@/styles/globals.css'

document.documentElement.classList.toggle('dark', localStorage.getItem('mini-brasfoot-theme') !== 'light')

createRoot(document.getElementById('root')!).render(<StrictMode><App /></StrictMode>)
