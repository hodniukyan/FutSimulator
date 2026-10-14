import { z } from 'zod'

const color = z.string().regex(/^#[0-9a-fA-F]{6}$/, 'Use o formato #RRGGBB.').length(7)

export const customTeamSchema = z.object({
  name: z.string().trim().min(1, 'Informe o nome do clube.').max(100, 'Use até 100 caracteres.'),
  country: z.string().trim().min(1, 'Selecione um país.').max(100),
  badgeTemplate: z.enum(['shield-hexagonal', 'shield-round', 'shield-triangular']),
  primaryColor: color,
  secondaryColor: color,
})

export type CustomTeamFormValues = z.infer<typeof customTeamSchema>
