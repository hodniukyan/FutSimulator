import { UniformeDoClube, type PropriedadesDoUniforme } from './UniformeDoClube'
import { opcoesDeUniforme } from '../modelos-de-uniforme'

export function PreviaDoUniforme(propriedades: PropriedadesDoUniforme) {
  const nomeDoModelo = opcoesDeUniforme.find(opcao => opcao.modelo === propriedades.modelo)?.nome ?? 'Liso'
  return <div className="mt-5 flex flex-col items-center gap-2 border-t pt-5">
    <h3 className="text-sm font-medium">Uniforme do clube</h3>
    <UniformeDoClube {...propriedades} tamanho={160} />
    <p className="text-xs text-muted-foreground">{nomeDoModelo} · Estilo 8-bit</p>
  </div>
}
