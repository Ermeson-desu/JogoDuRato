# Falhas de Arquitetura Observadas

Escopo
Esta lista foi montada a partir de uma leitura estatica do codigo no repositorio. Ela cobre falhas visiveis no codigo atual, mas nao garante exaustividade absoluta.

Falhas
- God class no fluxo principal. `Game1` centraliza carregamento de conteudo, controle de estados, regras de jogo, navegacao de telas, save/load e logica de mapas.
Evidencia: `GameMain/Core/Game1.cs`
- Acoplamento forte entre UI e nucleo do jogo. Telas fazem cast direto para `Game1` e chamam metodos de dominio.
Evidencia: `GameMain/UI/MenuScreen.cs`, `GameMain/UI/PreGameScreen.cs`, `GameMain/UI/LoadScreen.cs`, `GameMain/UI/NewMapMenuScreen.cs`, `GameMain/UI/ChapterSelectScreen.cs`
- Camadas misturadas em fases. A fase contem UI (tela de vitoria) e persistencia (save) dentro da logica do gameplay.
Evidencia: `GameMain/Fases/Fase01.cs`
- Estado global e dependencias ocultas via classes estaticas. `SaveManager`, `MapDataManager` e `MapListManager` concentram dados e I/O, e `MapListManager` guarda estado global (`CurrentMapName`, `ExportedMapName`).
Evidencia: `GameMain/Core/SaveManager.cs`, `GameMain/Core/MapDataManager.cs`, `GameMain/Core/MapListManager.cs`
- Persistencia feita direto em `Content/` com IO de arquivo e JSON sem abstracao. Mistura dados mutaveis com assets e torna o jogo dependente de caminho local.
Evidencia: `GameMain/Core/SaveManager.cs`, `GameMain/Core/MapDataManager.cs`, `GameMain/Core/MapListManager.cs`
- Duplicacao de responsabilidades no sistema de mapas. `MapListManager` e `MapDataManager` usam o mesmo `maps.json` com formatos diferentes, incluindo conversao de legado.
Evidencia: `GameMain/Core/MapListManager.cs`, `GameMain/Core/MapDataManager.cs`
- Editor embutido no runtime. O mapa-editor (`CreateMappingScreen`) faz parte do mesmo ciclo do jogo e divide estado com gameplay, sem modulo separado.
Evidencia: `GameMain/UI/CreateMappingScreen.cs`, `GameMain/Core/Game1.cs`
- Dependencias especificas de Windows dentro da UI. P/Invoke em `user32.dll` e uso de PowerShell para dialogo de arquivo; controle via SharpDX DirectInput.
Evidencia: `GameMain/UI/CreateMappingScreen.cs`, `GameMain/Utils/DirectInputController.cs`
- Excesso de numeros magicos e coordenadas fixas espalhadas. Dificulta ajuste para outras resolucoes e balanceamento.
Evidencia: `GameMain/Entities/Player.cs`, `GameMain/Fases/Fase01.cs`, `GameMain/UI/CreateMappingScreen.cs`
- Entidades com muitas responsabilidades. `Player` mistura input, fisica, colisao, animacao e regras de respawn.
Evidencia: `GameMain/Entities/Player.cs`
- Service Locator escondendo dependencias. Varios lugares obtendo `SpriteBatch` ou `GameTime` via `Game.Services` sem contratos claros.
Evidencia: `GameMain/Entities/Player.cs`, `GameMain/Fases/Fase01.cs`
- Ciclo de vida de recursos grafico nao centralizado. Texturas sao criadas/carregadas localmente sem descarte explicito, o que tende a vazar memoria quando telas recarregam.
Evidencia: `GameMain/UI/CreateMappingScreen.cs`, `GameMain/Entities/Obstacle.cs`, `GameMain/Entities/Player.cs`
- Input duplicado por tela e por entidade. Cada tela cria seu proprio `DirectInputController`, sem sistema de input unico.
Evidencia: `GameMain/UI/MenuScreen.cs`, `GameMain/UI/PreGameScreen.cs`, `GameMain/UI/LoadScreen.cs`, `GameMain/Utils/DirectInputController.cs`
- Modelagem de dados com strings soltas para tipos de collider. `DynamicFase` interpreta valores textuais sem validacao centralizada.
Evidencia: `GameMain/Fases/DynamicFase.cs`, `GameMain/Core/MapData.cs`
