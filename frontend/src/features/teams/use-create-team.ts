import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createCustomTeam } from './api'

export function useCreateTeam() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createCustomTeam,
    onSuccess: () => { void queryClient.invalidateQueries({ queryKey: ['teams'] }) },
  })
}
