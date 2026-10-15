import { Controller, useForm, useWatch } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Link } from 'react-router-dom'
import { ArrowLeft, LoaderCircle, Plus } from 'lucide-react'
import { Button, buttonVariants } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { BadgeSelector } from '@/features/teams/components/BadgeSelector'
import { BadgePreview } from '@/features/teams/components/BadgePreview'
import { TeamBadge } from '@/features/teams/components/TeamBadge'
import { ColorPicker } from '@/features/teams/components/ColorPicker'
import { SeletorDeUniforme } from '@/features/teams/components/SeletorDeUniforme'
import { PreviaDoUniforme } from '@/features/teams/components/PreviaDoUniforme'
import { UniformeDoClube } from '@/features/teams/components/UniformeDoClube'
import { customTeamSchema, type CustomTeamFormValues } from '@/features/teams/schema'
import { useCreateTeam } from '@/features/teams/use-create-team'
import { getHttpError } from '@/services/http'

const countries = ['Brasil', 'Argentina', 'Uruguai', 'Chile', 'Colômbia', 'Paraguai', 'Peru', 'Equador', 'Bolívia', 'Venezuela',
  'Portugal', 'Espanha', 'França', 'Alemanha', 'Itália', 'Inglaterra', 'Países Baixos', 'Bélgica', 'Estados Unidos', 'México', 'Japão', 'Coreia do Sul', 'Austrália', 'África do Sul']
const positions: Record<string, string> = { GK: 'Goleiro', DEF: 'Defensor', MEI: 'Meio-campista', ATT: 'Atacante' }
const inputClass = 'h-11 w-full rounded-md border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring'

export function CreateTeamPage() {
  const mutation = useCreateTeam()
  const { register, control, handleSubmit, reset, formState: { errors } } = useForm<CustomTeamFormValues>({
    resolver: zodResolver(customTeamSchema),
    defaultValues: { name: '', country: 'Brasil', badgeTemplate: 'shield-hexagonal', primaryColor: '#F97316', secondaryColor: '#17212B', modeloDoUniforme: 'uniforme-liso' },
  })
  const values = useWatch({ control })
  const primaryColor = values.primaryColor ?? '#F97316'
  const secondaryColor = values.secondaryColor ?? '#17212B'

  if (mutation.isSuccess) {
    const { team, players } = mutation.data
    return <>
      <p role="status" className="mb-4 text-sm font-medium text-primary">Clube criado com sucesso!</p>
      <Card className="gap-4 p-6">
        <div className="flex items-center gap-5"><TeamBadge template={team.badgeTemplate} primaryColor={team.primaryColor} secondaryColor={team.secondaryColor} name={team.name} size={80} />
          <div className="min-w-0 flex-1"><h1 className="break-words text-2xl font-semibold">{team.name}</h1><p className="mt-1 text-muted-foreground">{team.country} · Força {team.strength}</p></div>
          <UniformeDoClube modelo={team.modeloDoUniforme} corPrimaria={team.primaryColor} corSecundaria={team.secondaryColor} nomeDoClube={team.name} tamanho={64} /></div>
        <h2 className="mt-3 text-lg font-semibold">Seu elenco · 4-4-2</h2>
        <div className="overflow-x-auto"><table className="w-full text-left text-sm">
          <caption className="sr-only">Os 11 jogadores gerados para {team.name}</caption>
          <thead className="border-b text-muted-foreground"><tr><th scope="col" className="py-3 pr-4">Jogador</th><th scope="col" className="pr-4">Posição</th><th scope="col" className="pr-4">Idade</th><th scope="col">Habilidade</th></tr></thead>
          <tbody>{players.map(player => <tr key={player.id} className="border-b last:border-0"><td className="py-3 pr-4 font-medium">{player.name}</td><td className="pr-4">{positions[player.position] || player.position}</td><td className="pr-4 tabular-nums">{player.age}</td><td className="tabular-nums">{player.skill}</td></tr>)}</tbody>
        </table></div>
        <div className="flex flex-wrap gap-3 pt-3"><Link to="/" className={buttonVariants()}>Preparar amistoso</Link>
          <Link to="/clubs" className={buttonVariants({ variant: 'outline' })}>Ver clubes</Link>
          <Button variant="outline" onClick={() => { mutation.reset(); reset() }}><Plus /> Criar outro</Button></div>
      </Card>
    </>
  }

  return <>
    <Link to="/clubs" className="mb-5 inline-flex items-center gap-2 text-sm text-muted-foreground"><ArrowLeft size={16} /> Voltar aos clubes</Link>
    <h1 className="text-3xl font-semibold">Crie seu clube</h1>
    <p className="mt-3 text-muted-foreground">Escolha a identidade do time. Os 11 jogadores serão gerados automaticamente.</p>
    <div className="mt-8 grid items-start gap-6 lg:grid-cols-[1.3fr_1fr]">
      <Card className="gap-0 p-6"><form className="space-y-6" onSubmit={handleSubmit(input => { if (!mutation.isPending) mutation.mutate(input) })}>
        <div><label htmlFor="name" className="mb-2 block text-sm font-medium">Nome do clube</label>
          <input id="name" {...register('name')} maxLength={100} disabled={mutation.isPending} placeholder="Ex.: Aurora FC" className={inputClass}
            aria-invalid={!!errors.name} aria-describedby={errors.name ? 'name-error' : undefined} />
          {errors.name && <p id="name-error" className="mt-2 text-xs text-destructive">{errors.name.message}</p>}</div>
        <div><label htmlFor="country" className="mb-2 block text-sm font-medium">País</label>
          <select id="country" {...register('country')} disabled={mutation.isPending} className={inputClass}
            aria-invalid={!!errors.country} aria-describedby={errors.country ? 'country-error' : undefined}>
            <option value="">Selecionar país</option>{countries.map(country => <option key={country} value={country}>{country}</option>)}</select>
          {errors.country && <p id="country-error" className="mt-2 text-xs text-destructive">{errors.country.message}</p>}</div>
        <Controller name="badgeTemplate" control={control} render={({ field }) => <BadgeSelector value={field.value} onChange={field.onChange}
          primaryColor={primaryColor} secondaryColor={secondaryColor} disabled={mutation.isPending} />} />
        <div className="grid gap-4 sm:grid-cols-2">
          <Controller name="primaryColor" control={control} render={({ field }) => <ColorPicker id="primaryColor" label="Cor primária" value={field.value}
            onChange={field.onChange} disabled={mutation.isPending} error={errors.primaryColor?.message} />} />
          <Controller name="secondaryColor" control={control} render={({ field }) => <ColorPicker id="secondaryColor" label="Cor secundária" value={field.value}
            onChange={field.onChange} disabled={mutation.isPending} error={errors.secondaryColor?.message} />} />
        </div>
        <Controller name="modeloDoUniforme" control={control} render={({ field: campo }) => <SeletorDeUniforme
          modeloSelecionado={campo.value ?? 'uniforme-liso'} aoSelecionar={campo.onChange}
          corPrimaria={primaryColor} corSecundaria={secondaryColor} desabilitado={mutation.isPending} />} />
        {errors.modeloDoUniforme && <p role="alert" className="text-xs text-destructive">{errors.modeloDoUniforme.message}</p>}
        <p className="text-xs text-muted-foreground">Formação 4-4-2, jogadores de 18 a 32 anos e habilidades de 50 a 80. A força inicial será calculada a partir do elenco.</p>
        <Button type="submit" className="w-full" disabled={mutation.isPending}>{mutation.isPending ? <><LoaderCircle className="animate-spin" /> Criando clube...</> : <><Plus /> Criar clube</>}</Button>
        {mutation.isError && <p role="alert" className="text-sm text-destructive">{getHttpError(mutation.error)}</p>}
      </form></Card>
      <Card className="gap-0 p-6"><h2 className="text-lg font-semibold">Sua identidade</h2><p className="mt-1 text-sm text-muted-foreground">Prévia em tempo real</p>
        <BadgePreview template={values.badgeTemplate} primaryColor={primaryColor} secondaryColor={secondaryColor} name={values.name} />
        <p className="text-center text-sm text-muted-foreground">{values.country || 'Selecione um país'}</p>
        <PreviaDoUniforme modelo={values.modeloDoUniforme} corPrimaria={primaryColor} corSecundaria={secondaryColor} nomeDoClube={values.name?.trim() || 'Seu clube'} />
      </Card>
    </div>
  </>
}
