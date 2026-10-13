import { http } from '@/services/http'

export interface MatchResult {
  team1: string; team2: string; nivel: string; score: string
  goals: { minute: number; team: string; player: string }[]
}

export async function simulateMatch(team1Id: number, team2Id: number): Promise<MatchResult> {
  return (await http.post<MatchResult>('/match', null, { params: { team1Id, team2Id } })).data
}
