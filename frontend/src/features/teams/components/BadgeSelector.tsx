import { TeamBadge } from './TeamBadge'
import type { BadgeTemplate } from '../types'

const options: { value: BadgeTemplate; label: string }[] = [
  { value: 'shield-hexagonal', label: 'Hexagonal' },
  { value: 'shield-round', label: 'Redondo' },
  { value: 'shield-triangular', label: 'Triangular' },
]

type Props = {
  value: BadgeTemplate
  primaryColor: string
  secondaryColor: string
  onChange: (value: BadgeTemplate) => void
  disabled?: boolean
}

export function BadgeSelector({ value, primaryColor, secondaryColor, onChange, disabled }: Props) {
  return <fieldset disabled={disabled}>
    <legend className="mb-3 text-sm font-medium">Modelo de escudo</legend>
    <div className="grid grid-cols-3 gap-3">
      {options.map(option => <label key={option.value} className="relative cursor-pointer">
        <input type="radio" name="badgeTemplate" value={option.value} checked={value === option.value}
          onChange={() => onChange(option.value)} className="peer sr-only" />
        <span className="flex h-full flex-col items-center gap-2 rounded-lg border p-3 text-xs peer-checked:border-primary peer-checked:bg-primary/5 peer-focus-visible:ring-2 peer-focus-visible:ring-ring peer-disabled:opacity-60">
          <TeamBadge template={option.value} primaryColor={primaryColor} secondaryColor={secondaryColor} size={52} name={option.label} />
          {option.label}
        </span>
      </label>)}
    </div>
  </fieldset>
}
