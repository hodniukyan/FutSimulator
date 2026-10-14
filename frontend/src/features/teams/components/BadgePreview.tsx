import { TeamBadge, type TeamBadgeProps } from './TeamBadge'

export function BadgePreview({ name, ...props }: TeamBadgeProps) {
  return <div className="flex flex-col items-center gap-5 py-6">
    <TeamBadge {...props} name={name || 'Seu clube'} size={144} />
    <p className="max-w-full break-words text-center text-xl font-semibold">{name?.trim() || 'Seu clube'}</p>
  </div>
}
