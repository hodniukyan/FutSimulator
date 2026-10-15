import { renderToStaticMarkup } from 'react-dom/server'
import { describe, expect, it } from 'vitest'
import horizontal from '@/assets/kits/uniforme-listrado-horizontal.svg?raw'
import vertical from '@/assets/kits/uniforme-listrado-vertical.svg?raw'
import liso from '@/assets/kits/uniforme-liso.svg?raw'
import meioAMeio from '@/assets/kits/uniforme-meio-a-meio.svg?raw'
import { UniformeDoClube } from './UniformeDoClube'
import { SeletorDeUniforme } from './SeletorDeUniforme'
import { PreviaDoUniforme } from './PreviaDoUniforme'
import { customTeamSchema } from '../schema'
import { identificadoresDeUniforme, type ModeloDeUniforme } from '../modelos-de-uniforme'

const desenhosDeUniforme = [horizontal, vertical, liso, meioAMeio]

describe('Uniformes dos clubes', () => {
  it.each(identificadoresDeUniforme)('renderiza %s como SVG inline com cores locais', modelo => {
    const resultado = renderToStaticMarkup(<UniformeDoClube modelo={modelo} corPrimaria="#165DFF" corSecundaria="#FFD700" nomeDoClube="Aurora" tamanho={128} />)
    expect(resultado).toContain('<svg')
    expect(resultado).not.toContain('<img')
    expect(resultado).toContain('viewBox="0 0 64 64"')
    expect(resultado).toContain('shape-rendering="crispEdges"')
    expect(resultado).toContain('--primary-color:#165DFF;--secondary-color:#FFD700;width:128px;height:128px')
    expect(resultado).toContain('aria-label="Uniforme de Aurora"')
  })

  it('mantém desenhos pixelados sem curvas, gradientes, filtros ou IDs duplicáveis', () => {
    for (const desenho of desenhosDeUniforme) {
      expect(desenho).not.toMatch(/<(?:filter|linearGradient|radialGradient|image|clipPath|mask)\b/)
      expect(desenho).not.toMatch(/\bid=/)
      for (const caminho of desenho.matchAll(/\bd="([^"]+)"/g)) {
        expect(caminho[1]).toMatch(/^[MHVZ\d\s-]+$/)
      }
    }
    expect(new Set(desenhosDeUniforme.map(desenho => desenho.match(/\bd="([^"]+)"/)![1])).size).toBe(1)
  })

  it('usa a secundária somente nos modelos bicolores', () => {
    expect(liso).toContain('var(--primary-color')
    expect(liso).not.toContain('--secondary-color')
    for (const desenho of [horizontal, vertical, meioAMeio]) {
      expect(desenho).toContain('var(--secondary-color')
      expect(desenho).toContain('var(--primary-color')
    }
  })

  it('mantém as instâncias independentes e usa liso para clubes anteriores', () => {
    const resultado = renderToStaticMarkup(<>
      <UniformeDoClube modelo="uniforme-meio-a-meio" corPrimaria="#112233" corSecundaria="#445566" />
      <UniformeDoClube modelo="uniforme-meio-a-meio" corPrimaria="#AABBCC" corSecundaria="#DDEEFF" />
    </>)
    expect(resultado).toContain('--primary-color:#112233;--secondary-color:#445566')
    expect(resultado).toContain('--primary-color:#AABBCC;--secondary-color:#DDEEFF')
    const padrao = renderToStaticMarkup(<UniformeDoClube />)
    const desconhecido = renderToStaticMarkup(<UniformeDoClube modelo={'desconhecido' as ModeloDeUniforme} />)
    expect(desconhecido).toBe(padrao)
    expect(padrao).toContain('--primary-color:#94A3B8')
  })

  it('rejeita cores inválidas e mantém o uniforme utilizável', () => {
    const resultado = renderToStaticMarkup(<UniformeDoClube corPrimaria="url(https://example.com)" corSecundaria="#123456\n" />)
    expect(resultado).toContain('--primary-color:#94A3B8;--secondary-color:#475569')
    expect(resultado).not.toContain('example.com')
  })

  it('oferece quatro opções acessíveis e uma prévia com o modelo escolhido', () => {
    const seletor = renderToStaticMarkup(<SeletorDeUniforme modeloSelecionado="uniforme-liso"
      corPrimaria="#123456" corSecundaria="#ABCDEF" aoSelecionar={() => {}} desabilitado />)
    expect([...seletor.matchAll(/type="radio"/g)]).toHaveLength(4)
    expect([...seletor.matchAll(/ checked=""/g)]).toHaveLength(1)
    expect(seletor).toContain('fieldset disabled=""')
    expect(seletor).toContain('Camisa lisa, usando apenas a cor primária.')
    expect(renderToStaticMarkup(<PreviaDoUniforme modelo="uniforme-meio-a-meio" nomeDoClube="Aurora" />)).toContain('Meio a meio')
  })

  it('valida os modelos do formulário preservando requisições anteriores', () => {
    const entrada = { name: 'Aurora', country: 'Brasil', badgeTemplate: 'shield-round', primaryColor: '#123456', secondaryColor: '#ABCDEF' }
    expect(customTeamSchema.safeParse(entrada).success).toBe(true)
    for (const modelo of identificadoresDeUniforme) {
      expect(customTeamSchema.parse({ ...entrada, modeloDoUniforme: modelo }).modeloDoUniforme).toBe(modelo)
    }
    for (const modeloInvalido of ['desconhecido', '', null, 'uniforme-liso\n']) {
      expect(customTeamSchema.safeParse({ ...entrada, modeloDoUniforme: modeloInvalido }).success).toBe(false)
    }
  })
})
