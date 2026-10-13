import { useQuery } from '@tanstack/react-query'
import { getTeams } from './api'

export function useTeams() {
  return useQuery({ queryKey: ['teams'], queryFn: ({ signal }) => getTeams(signal) })
}
