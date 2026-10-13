import { z } from 'zod'

export const matchSchema = z.object({
  team1Id: z.string().min(1, 'Selecione o time da casa.'),
  team2Id: z.string().min(1, 'Selecione o time visitante.'),
}).refine(({ team1Id, team2Id }) => team1Id !== team2Id, {
  message: 'Selecione dois times diferentes.', path: ['team2Id'],
})
export type MatchFormValues = z.infer<typeof matchSchema>
