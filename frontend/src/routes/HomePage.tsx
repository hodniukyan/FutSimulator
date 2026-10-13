import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation } from '@tanstack/react-query'
import { ArrowRight, LoaderCircle, RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useTeams } from '@/features/teams/use-teams'
import { simulateMatch } from '@/features/matches/api'
import { matchSchema, type MatchFormValues } from '@/features/matches/schema'
import { getHttpError } from '@/services/http'

export function HomePage() {
  const teams = useTeams()
  const match = useMutation({ mutationFn: (values: MatchFormValues) => simulateMatch(Number(values.team1Id), Number(values.team2Id)) })
  const { register, handleSubmit, formState: { errors } } = useForm<MatchFormValues>({
    resolver: zodResolver(matchSchema), defaultValues: { team1Id: '', team2Id: '' },
  })
  return <>
    <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-primary">Central do treinador</p>
    <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">O próximo jogo está nas suas mãos.</h1>
    <p className="mt-3 max-w-xl text-muted-foreground">Escolha os clubes e acompanhe o resultado de uma partida amistosa.</p>
    <div className="mt-9 grid items-start gap-6 lg:grid-cols-[1.2fr_1fr]">
      <Card className="gap-0 p-6" aria-labelledby="match-title">
        <h2 id="match-title" className="text-lg font-semibold">Preparar partida</h2>
        <p className="mt-1 text-sm text-muted-foreground">Cada equipe precisa ter jogadores de linha cadastrados.</p>
        {teams.isPending && <p role="status" className="mt-6 flex items-center gap-2 text-sm"><LoaderCircle className="size-4 animate-spin" /> Carregando clubes...</p>}
        {teams.isError && <div className="mt-6" role="alert">
          <p className="text-sm text-destructive">{getHttpError(teams.error)}</p>
          <Button variant="outline" className="mt-3" onClick={() => void teams.refetch()} disabled={teams.isFetching}><RotateCcw /> Tentar novamente</Button>
        </div>}
        {teams.isSuccess && teams.data.length < 2 && <p className="mt-6 text-sm text-muted-foreground">Cadastre pelo menos dois clubes na API para começar.</p>}
        {teams.isSuccess && teams.data.length >= 2 && <form className="mt-6 space-y-5" onSubmit={handleSubmit(values => { if (!match.isPending) match.mutate(values) })}>
          <div className="grid gap-4 sm:grid-cols-2">
            {(['team1Id', 'team2Id'] as const).map((field, index) => <div key={field}>
              <label htmlFor={field} className="mb-2 block text-sm font-medium">{index === 0 ? 'Time da casa' : 'Visitante'}</label>
              <select id={field} {...register(field)} disabled={match.isPending} aria-invalid={!!errors[field]} aria-describedby={errors[field] ? `${field}-error` : undefined}
                className="h-11 w-full rounded-md border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
                <option value="">Selecionar clube</option>
                {teams.data.map(team => <option key={team.id} value={team.id}>{team.name}</option>)}
              </select>
              {errors[field] && <p id={`${field}-error`} className="mt-2 text-xs text-destructive">{errors[field]?.message}</p>}
            </div>)}
          </div>
          <Button type="submit" className="w-full" disabled={match.isPending}>
            {match.isPending ? <><LoaderCircle className="animate-spin" /> Simulando...</> : <>Simular partida <ArrowRight /></>}
          </Button>
          {match.isError && <p role="alert" className="text-sm text-destructive">{getHttpError(match.error)}</p>}
        </form>}
      </Card>
      <Card className="gap-0 p-6" aria-labelledby="result-title">
        <h2 id="result-title" className="text-lg font-semibold">Resultado</h2>
        <div aria-live="polite" aria-busy={match.isPending}>
          {match.isPending ? <p className="py-10 text-center text-sm text-muted-foreground">A bola está rolando...</p>
            : match.isSuccess ? <>
              <div className="py-7 text-center">
                <p className="text-sm text-muted-foreground">{match.data.team1} × {match.data.team2}</p>
                <p className="mt-2 text-5xl font-semibold tabular-nums text-primary">{match.data.score}</p>
              </div>
              {match.data.goals.length === 0 ? <p className="text-sm text-muted-foreground">A partida terminou sem gols.</p>
                : <ol className="space-y-3 border-t pt-4">{match.data.goals.map((goal, index) =>
                  <li key={index} className="flex gap-3 text-sm"><span className="w-9 font-medium tabular-nums text-primary">{goal.minute}'</span>
                    <span>{goal.player}<span className="block text-xs text-muted-foreground">{goal.team}</span></span></li>)}</ol>}
            </> : <p className="py-10 text-center text-sm text-muted-foreground">O placar e os autores dos gols aparecerão aqui.</p>}
        </div>
      </Card>
    </div>
  </>
}
