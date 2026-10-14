import type { CSSProperties } from 'react'
import HexagonalShield from '@/assets/shields/shield-hexagonal.svg?react'
import RoundShield from '@/assets/shields/shield-round.svg?react'
import TriangularShield from '@/assets/shields/shield-triangular.svg?react'
import type { BadgeTemplate } from '../types'

const templates = {
  'shield-hexagonal': HexagonalShield,
  'shield-round': RoundShield,
  'shield-triangular': TriangularShield,
}

type BadgeStyle = CSSProperties & { '--primary-color': string; '--secondary-color': string }
export type TeamBadgeProps = {
  template?: BadgeTemplate | null
  primaryColor?: string | null
  secondaryColor?: string | null
  size?: number
  name?: string
}

export function TeamBadge({ template, primaryColor, secondaryColor, size = 56, name = 'Clube' }: TeamBadgeProps) {
  const Shield = template && Object.hasOwn(templates, template) ? templates[template] : undefined
  const validColor = (color: string | null | undefined, fallback: string) =>
    color && /^#[0-9a-fA-F]{6}$/.test(color) && color.length === 7 ? color : fallback
  const style: BadgeStyle = {
    '--primary-color': validColor(primaryColor, '#F97316'),
    '--secondary-color': validColor(secondaryColor, '#17212B'),
    width: size, height: size * 140 / 120, flexShrink: 0,
  }
  if (!Shield) {
    const initials = name.trim().split(/\s+/).slice(0, 2).map(word => word[0]).join('').toUpperCase() || 'FC'
    return <span role="img" aria-label={`Clube ${name}, sem escudo personalizado`} style={style}
      className="inline-flex items-center justify-center rounded-lg border bg-muted text-sm font-semibold text-muted-foreground">{initials}</span>
  }
  return <Shield style={style} className="inline-block" role="img" aria-label={`Escudo de ${name}`} />
}
