type Props = { id: string; label: string; value: string; onChange: (color: string) => void; disabled?: boolean; error?: string }

export function ColorPicker({ id, label, value, onChange, disabled, error }: Props) {
  const valid = /^#[0-9a-fA-F]{6}$/.test(value) && value.length === 7
  return <div>
    <label htmlFor={id} className="mb-2 block text-sm font-medium">{label}</label>
    <div className="flex gap-2">
      <input type="color" value={valid ? value : '#F97316'} disabled={disabled} onChange={event => onChange(event.target.value)}
        aria-label={`Selecionar ${label.toLowerCase()}`} className="h-11 w-12 shrink-0 cursor-pointer rounded-md border border-input bg-background p-1" />
      <input id={id} value={value} onChange={event => onChange(event.target.value)} disabled={disabled} maxLength={7}
        spellCheck={false} aria-invalid={!!error} aria-describedby={error ? `${id}-error` : undefined}
        className="h-11 min-w-0 w-full rounded-md border border-input bg-background px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring" />
    </div>
    {error && <p id={`${id}-error`} className="mt-2 text-xs text-destructive">{error}</p>}
  </div>
}
