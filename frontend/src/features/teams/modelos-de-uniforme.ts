export const identificadoresDeUniforme = [
  'uniforme-listrado-horizontal',
  'uniforme-listrado-vertical',
  'uniforme-liso',
  'uniforme-meio-a-meio',
] as const

export type ModeloDeUniforme = typeof identificadoresDeUniforme[number]

export const opcoesDeUniforme: { modelo: ModeloDeUniforme; nome: string; descricao: string }[] = [
  { modelo: 'uniforme-listrado-horizontal', nome: 'Horizontal', descricao: 'Listras horizontais nas duas cores do clube.' },
  { modelo: 'uniforme-listrado-vertical', nome: 'Vertical', descricao: 'Listras verticais nas duas cores do clube.' },
  { modelo: 'uniforme-liso', nome: 'Liso', descricao: 'Camisa lisa, usando apenas a cor primária.' },
  { modelo: 'uniforme-meio-a-meio', nome: 'Meio a meio', descricao: 'Uma metade de cada cor, com mangas correspondentes.' },
]
