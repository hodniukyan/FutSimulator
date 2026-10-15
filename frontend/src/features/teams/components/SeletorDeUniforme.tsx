import { opcoesDeUniforme, type ModeloDeUniforme } from '../modelos-de-uniforme'
import { UniformeDoClube } from './UniformeDoClube'

type PropriedadesDoSeletor = {
  modeloSelecionado: ModeloDeUniforme
  corPrimaria: string
  corSecundaria: string
  aoSelecionar: (modelo: ModeloDeUniforme) => void
  desabilitado?: boolean
}

export function SeletorDeUniforme({ modeloSelecionado, corPrimaria, corSecundaria, aoSelecionar, desabilitado }: PropriedadesDoSeletor) {
  const descricaoSelecionada = opcoesDeUniforme.find(opcao => opcao.modelo === modeloSelecionado)?.descricao
  return <fieldset disabled={desabilitado}>
    <legend className="mb-3 text-sm font-medium">Modelo de uniforme</legend>
    <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
      {opcoesDeUniforme.map(opcao => <label key={opcao.modelo} className="relative cursor-pointer">
        <input type="radio" name="modeloDoUniforme" value={opcao.modelo} checked={modeloSelecionado === opcao.modelo}
          onChange={() => aoSelecionar(opcao.modelo)} aria-label={opcao.nome} className="peer sr-only" />
        <span className="flex h-full flex-col items-center gap-2 rounded-lg border p-3 text-xs peer-checked:border-primary peer-checked:bg-primary/5 peer-focus-visible:ring-2 peer-focus-visible:ring-ring peer-disabled:opacity-60">
          <UniformeDoClube modelo={opcao.modelo} corPrimaria={corPrimaria} corSecundaria={corSecundaria} tamanho={56} nomeDoClube={opcao.nome} />
          {opcao.nome}
        </span>
      </label>)}
    </div>
    <p className="mt-3 text-xs text-muted-foreground">{descricaoSelecionada} As cores acompanham o escudo.</p>
  </fieldset>
}
