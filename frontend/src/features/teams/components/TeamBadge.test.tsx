import { createHash } from 'node:crypto'
import { renderToStaticMarkup } from 'react-dom/server'
import { describe, expect, it } from 'vitest'
import hexagonal from '@/assets/shields/shield-hexagonal.svg?raw'
import round from '@/assets/shields/shield-round.svg?raw'
import triangular from '@/assets/shields/shield-triangular.svg?raw'
import { TeamBadge } from './TeamBadge'
import type { BadgeTemplate } from '../types'

const originals: { template: BadgeTemplate; svg: string; hash: string }[] = [
  { template: 'shield-hexagonal', svg: hexagonal, hash: '48ac1c7ef029018b91f995348f49dae91301e838f59d8406303f1ba7a01e265b' },
  { template: 'shield-round', svg: round, hash: '5fa8d63d2ea7075d1fd518de4d18b9cb563fa14fdf11f69b99046d09ce7118ca' },
  { template: 'shield-triangular', svg: triangular, hash: 'c5cdc8a4d4c9e96036242ecdc1687c6198c5f825518dbd2cfb8d622ac0429374' },
]

describe('TeamBadge', () => {
  it.each(originals)('preserves the supplied $template artwork and inline color variables', ({ template, svg, hash }) => {
    expect(createHash('sha256').update(svg).digest('hex')).toBe(hash)
    const markup = renderToStaticMarkup(<TeamBadge template={template} primaryColor="#123456" secondaryColor="#ABCDEF" name="Aurora FC" />)
    expect(markup).toContain('<svg')
    expect(markup).not.toContain('<img')
    expect(markup).toContain('viewBox="0 0 120 140"')
    expect(markup).toContain('--primary-color:#123456')
    expect(markup).toContain('--secondary-color:#ABCDEF')
    expect(markup).toContain('var(--primary-color, #F97316)')
    expect(markup).toContain('var(--secondary-color, #17212B)')
    expect(markup).toContain('#FFFFFF')
    expect(markup).toContain('aria-label="Escudo de Aurora FC"')
    // Compare every original element and drawing attribute with the imported React SVG.
    for (const element of svg.matchAll(/<(path|circle)\s+([^>]+)\/>/g)) {
      const renderedElements = [...markup.matchAll(new RegExp(`<${element[1]}\\s+([^>]+)>`, 'g'))]
      expect(renderedElements.some(rendered => [...element[2].matchAll(/([\w-]+)="([^"]*)"/g)]
        .every(attribute => rendered[1].includes(`${attribute[1]}="${attribute[2]}"`)))).toBe(true)
    }
    expect([...markup.matchAll(/<(path|circle)\s/g)]).toHaveLength([...svg.matchAll(/<(path|circle)\s/g)].length)
  })

  it('keeps colors independent for multiple clubs and preserves proportions', () => {
    const markup = renderToStaticMarkup(<>
      <TeamBadge template="shield-round" primaryColor="#112233" secondaryColor="#445566" size={120} />
      <TeamBadge template="shield-round" primaryColor="#778899" secondaryColor="#AABBCC" size={60} />
    </>)
    expect(markup).toContain('--primary-color:#112233;--secondary-color:#445566;width:120px;height:140px')
    expect(markup).toContain('--primary-color:#778899;--secondary-color:#AABBCC;width:60px;height:70px')
  })

  it('uses a readable fallback for existing clubs or unknown templates', () => {
    expect(renderToStaticMarkup(<TeamBadge name="Clube Legado" />)).toContain('>CL</span>')
    expect(renderToStaticMarkup(<TeamBadge template={'unknown' as BadgeTemplate} name="Aurora" />)).not.toContain('<svg')
  })

  it('rejects invalid CSS colors while retaining a usable shield', () => {
    const markup = renderToStaticMarkup(<TeamBadge template="shield-triangular" primaryColor="url(https://example.com)" secondaryColor="#123456\n" />)
    expect(markup).toContain('--primary-color:#F97316;--secondary-color:#17212B')
    expect(markup).not.toContain('example.com')
  })
})
