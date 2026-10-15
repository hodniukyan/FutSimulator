import type { CSSProperties } from 'react'
import UniformeHorizontal from '@/assets/kits/uniforme-listrado-horizontal.svg?react'
import UniformeVertical from '@/assets/kits/uniforme-listrado-vertical.svg?react'
import UniformeLiso from '@/assets/kits/uniforme-liso.svg?react'
import UniformeMeioAMeio from '@/assets/kits/uniforme-meio-a-meio.svg?react'
import type { ModeloDeUniforme } from '../modelos-de-uniforme'

const componentesDeUniforme = {
  'uniforme-listrado-horizontal': UniformeHorizontal,
  'uniforme-listrado-vertical': UniformeVertical,
  'uniforme-liso': UniformeLiso,
  'uniforme-meio-a-meio': UniformeMeioAMeio,
}

type EstiloDoUniforme = CSSProperties & { '--primary-color': string; '--secondary-color': string }

export type PropriedadesDoUniforme = {
  modelo?: ModeloDeUniforme | null
  corPrimaria?: string | null
  corSecundaria?: string | null
  tamanho?: number
  nomeDoClube?: string
}

function obterCorValida(cor: string | null | undefined, corPadrao: string) {
  return cor && /^#[0-9a-fA-F]{6}$/.test(cor) && cor.length === 7 ? cor : corPadrao
}

export function UniformeDoClube({ modelo, corPrimaria, corSecundaria, tamanho = 64, nomeDoClube = 'Seu clube' }: PropriedadesDoUniforme) {
  const modeloConhecido = modelo && Object.hasOwn(componentesDeUniforme, modelo) ? modelo : 'uniforme-liso'
  const Uniforme = componentesDeUniforme[modeloConhecido]
  const estilo: EstiloDoUniforme = {
    '--primary-color': obterCorValida(corPrimaria, '#94A3B8'),
    '--secondary-color': obterCorValida(corSecundaria, '#475569'),
    width: tamanho, height: tamanho, flexShrink: 0,
  }
  return <Uniforme style={estilo} className="inline-block" role="img" aria-label={`Uniforme de ${nomeDoClube}`} />
}
