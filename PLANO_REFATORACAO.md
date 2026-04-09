# Plano de Refatoracao (por fases)

Objetivo
- Refatorar em partes pequenas, com validacao a cada passo, evitando quebrar outras areas.

Fase 0 — Baseline e seguranca
- Objetivo: garantir build e fluxo principal estaveis.
- Acoes:
  - Validar build.
  - Revisar DOCUMENTACAO.md.
  - Listar fluxos criticos (Menu -> Jogo -> Victory).
- Criterio de aceite: `dotnet build GameDuMouse.csproj` passa e o fluxo principal funciona.

Fase 1 — Input centralizado
- Objetivo: remover input duplicado e dependencias escondidas.
- Acoes:
  - Usar InputManager em todas as telas e entidades.
  - Remover criacao de input por tela.
- Arquivos-chave: GameMain/Input/InputManager.cs, GameMain/UI/*.cs, GameMain/Entities/Player.cs.
- Criterio de aceite: teclado, mouse e controle funcionam igual nas telas principais.

Fase 2 — Ciclo de vida de recursos
- Objetivo: eliminar vazamentos e criacao de textura em Draw.
- Acoes:
  - Padronizar TextureCache e AssetManager.
  - Remover new Texture2D dentro de Draw.
- Arquivos-chave: GameMain/Rendering/TextureCache.cs, GameMain/Services/AssetManager.cs, GameMain/UI/CreateMappingScreen.cs.
- Criterio de aceite: nao existe new Texture2D em Draw e nao ha queda de performance.

Fase 3 — Persistencia e cache
- Objetivo: substituir estaticos por servicos e evitar IO no loop.
- Acoes:
  - Migrar MapListManager, MapDataManager, SaveManager para MapService/SaveService.
  - Remover leitura e escrita direta em Update.
- Arquivos-chave: GameMain/Services/MapService.cs, GameMain/Services/SaveService.cs, telas de UI.
- Criterio de aceite: mapas e saves funcionam sem travar o loop.

Fase 4 — Desacoplamento da UI
- Objetivo: telas nao chamam Game1 diretamente.
- Acoes:
  - Criar interface de fluxo (ex: IGameFlow) e injetar via Game.Services.
  - Remover casts para Game1.
- Arquivos-chave: GameMain/UI/*.cs, GameMain/Managers/GameManager.cs.
- Criterio de aceite: nenhuma UI faz cast para Game1.

Fase 5 — Separacao de gameplay
- Objetivo: reduzir responsabilidades do Player e das fases.
- Acoes:
  - Separar input, fisica, colisao e animacao em systems/ componentes.
- Arquivos-chave: GameMain/Entities/Player.cs, GameMain/Systems/* (novo).
- Criterio de aceite: Player fica simples e regras ficam em systems dedicados.

Fase 6 — Editor isolado
- Objetivo: editor nao mistura estado com gameplay.
- Acoes:
  - Criar modulo/estado dedicado para editor.
  - Mover IO e dialogos para services.
- Arquivos-chave: GameMain/UI/CreateMappingScreen.cs, GameMain/Services/*.
- Criterio de aceite: editor funciona sem afetar o loop do jogo principal.

Fase 7 — Dados tipados
- Objetivo: remover strings soltas para tipos e validacao central.
- Acoes:
  - Usar enums para collider e objetos.
  - Validar no carregamento.
- Arquivos-chave: GameMain/Core/MapData.cs, GameMain/Fases/DynamicFase.cs.
- Criterio de aceite: nao ha comparacao com "Ground" ou strings similares.

Fase 8 — Numeros magicos e resolucao
- Objetivo: parametrizar layout e coordenadas.
- Acoes:
  - Centralizar valores em config.
  - Usar layout relativo.
- Arquivos-chave: GameMain/Entities/Player.cs, GameMain/Fases/Fase01.cs, GameMain/UI/CreateMappingScreen.cs.
- Criterio de aceite: mudar resolucao nao quebra layout.

Fase 9 — Performance e culling
- Objetivo: reduzir update e draw fora da tela.
- Acoes:
  - Expandir RenderContext para entidades e plataformas.
  - Culling em fases com muitos elementos.
- Arquivos-chave: GameMain/Rendering/RenderContext.cs, entidades e fases.
- Criterio de aceite: menos draw calls em cenas grandes.

Observacao
- Cada fase deve ser executada isoladamente, com build e teste rapido antes de seguir.
