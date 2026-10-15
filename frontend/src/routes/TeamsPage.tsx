import { Link } from 'react-router-dom'
import { Plus, RotateCcw } from 'lucide-react'
import { buttonVariants, Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useTeams } from '@/features/teams/use-teams'
import { TeamBadge } from '@/features/teams/components/TeamBadge'
import { UniformeDoClube } from '@/features/teams/components/UniformeDoClube'
import { getHttpError } from '@/services/http'

export function TeamsPage() {
  const teams = useTeams()
  return <>
    <div className="flex flex-wrap items-center justify-between gap-4">
      <div><h1 className="text-3xl font-semibold">Clubes</h1><p className="mt-2 text-muted-foreground">Encontre seu time ou crie uma nova história.</p></div>
      <Link to="/clubs/new" className={buttonVariants()}><Plus /> Criar clube</Link>
    </div>
    {teams.isPending && <p role="status" className="mt-8">Carregando clubes...</p>}
    {teams.isError && <div role="alert" className="mt-8"><p className="text-destructive">{getHttpError(teams.error)}</p>
      <Button className="mt-3" variant="outline" disabled={teams.isFetching} onClick={() => void teams.refetch()}><RotateCcw /> Tentar novamente</Button></div>}
    {teams.isSuccess && teams.data.length === 0 && <p className="mt-8 text-muted-foreground">Nenhum clube cadastrado. Crie seu primeiro clube.</p>}
    {teams.isSuccess && <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {teams.data.map(team => <Card key={team.id} className="gap-3 p-5">
        <div className="flex items-center gap-4"><TeamBadge template={team.badgeTemplate} primaryColor={team.primaryColor} secondaryColor={team.secondaryColor} name={team.name} />
          <div className="min-w-0 flex-1"><h2 className="break-words font-semibold">{team.name}</h2><p className="text-sm text-muted-foreground">{team.country}</p></div>
          <UniformeDoClube modelo={team.modeloDoUniforme} corPrimaria={team.primaryColor} corSecundaria={team.secondaryColor} nomeDoClube={team.name} tamanho={48} /></div>
        <div className="flex justify-between gap-3 border-t pt-3 text-xs text-muted-foreground"><span>{team.isCustom ? 'Personalizado' : 'Clube existente'}</span><span>Força {team.strength}</span></div>
      </Card>)}
    </div>}
  </>
}
