import { http } from '@/services/http'
import type { CustomTeamInput, CustomTeamResult, Team } from './types'

export async function getTeams(signal?: AbortSignal): Promise<Team[]> {
  return (await http.get<Team[]>('/teams', { signal })).data
}

export async function createCustomTeam(input: CustomTeamInput): Promise<CustomTeamResult> {
  return (await http.post<CustomTeamResult>('/teams/custom', input)).data
}
