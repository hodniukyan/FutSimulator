import { describe, expect, it } from 'vitest'
import { customTeamSchema } from './schema'

const valid = { name: ' Aurora FC ', country: ' Brasil ', badgeTemplate: 'shield-round', primaryColor: '#ff6b00', secondaryColor: '#111827' }

describe('custom club form', () => {
  it('trims required fields and sends only club identity data', () => {
    const result = customTeamSchema.parse({ ...valid, strength: 999, isCustom: false })
    expect(result.name).toBe('Aurora FC')
    expect(result.country).toBe('Brasil')
    expect(result).not.toHaveProperty('strength')
    expect(result).not.toHaveProperty('isCustom')
  })

  it.each([
    { name: ' ' }, { country: '' }, { name: 'a'.repeat(101) }, { badgeTemplate: 'unknown' },
    { primaryColor: 'red' }, { primaryColor: '#FFF' }, { primaryColor: '#12345Z' },
    { primaryColor: '#123456\n' }, { secondaryColor: '#12345678' },
  ])('rejects invalid club identity: %j', change => {
    expect(customTeamSchema.safeParse({ ...valid, ...change }).success).toBe(false)
  })
})
