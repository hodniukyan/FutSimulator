import type { Player } from '@/features/players/types'

export type BadgeTemplate = 'shield-hexagonal' | 'shield-round' | 'shield-triangular'

export interface Team {
  id: number
  name: string
  country: string
  strength: number
  isCustom?: boolean
  badgeTemplate?: BadgeTemplate | null
  primaryColor?: string | null
  secondaryColor?: string | null
}

export interface CustomTeamInput {
  name: string
  country: string
  badgeTemplate: BadgeTemplate
  primaryColor: string
  secondaryColor: string
}

export interface CustomTeamResult { team: Team; players: Player[] }
