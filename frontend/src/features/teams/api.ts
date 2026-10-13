import { http } from '@/services/http'
import type { Team } from './types'

export async function getTeams(signal?: AbortSignal): Promise<Team[]> {
  return (await http.get<Team[]>('/teams', { signal })).data
}
