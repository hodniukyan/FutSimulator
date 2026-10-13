import { http } from '@/services/http'
import type { Player } from './types'

export async function getPlayers(teamId: number, signal?: AbortSignal): Promise<Player[]> {
  return (await http.get<Player[]>(`/teams/${teamId}/players`, { signal })).data
}
