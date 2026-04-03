# Documentacao Detalhada do Projeto (Estilo Framework)

Este documento descreve a arquitetura, os modulos e os pontos de extensao do projeto, no estilo de uma documentacao de framework.

---

## 1) Visao Geral

O projeto e um jogo 2D feito em C# com MonoGame. Ele inclui:
- Jogo principal (gameplay)
- Editor de mapas integrado
- Sistema de saves

A arquitetura foi refatorada para:
- Evitar vazamentos de memoria
- Evitar IO bloqueante no loop
- Centralizar servicos (input, assets, IO)
- Reduzir acoplamento entre UI e core

---

## 2) Requisitos e Build

**Requisitos**
- Windows
- .NET 8.0 (net8.0-windows)
- MonoGame.Framework.WindowsDX 3.8.x
- SharpDX.DirectInput 4.2.0

**Build**
```powershell
 dotnet build GameDuMouse.csproj
```

**Run**
```powershell
 dotnet run --project GameDuMouse.csproj
```

**Dev Loop (auto restart)**
```powershell
 dotnet watch run --project GameDuMouse.csproj
```

---

## 3) Estrutura de Pastas

- `GameMain/Core`    — Tipos base do jogo (state, dados, camera)
- `GameMain/Entities` — Entidades do mundo (Player, Obstacle, etc.)
- `GameMain/Components` — Componentes de estado (PlayerBody, etc.)
- `GameMain/Fases`    — Fases e factory
- `GameMain/Systems`  — Systems de gameplay (input, fisica, animacao, colisao)
- `GameMain/UI`       — Telas e componentes UI
- `GameMain/Input`    — Input centralizado
- `GameMain/Managers` — Fluxo do jogo (IGameFlow)
- `GameMain/Services` — Assets e IO (cacheados)
- `GameMain/Rendering`— Cache de texturas
- `GameMain/Utils`    — Utilitarios gerais
- `Content`           — Assets e dados (Content Pipeline + JSON)

---

## 4) Fluxo de Inicializacao

**Game1 (bootstrap)**
- Cria `GraphicsDeviceManager`.
- Registra servicos em `Game.Services`:
  - `InputManager`
  - `TextureCache`
  - `AssetManager`
  - `MapService`
  - `SaveService`
  - `IGameFlow` (via `GameManager`)
- Instancia e carrega todas as telas
- Controla o fluxo de estados via `StateManager`

**GameManager (IGameFlow)**
- Adaptador de fluxo para a UI (StartNewGame, LoadSave, etc.)

---

## 5) Ciclo de Jogo (Loop)

- `Update(GameTime)` sempre usa `deltaTime` (TotalSeconds)
- `Draw(GameTime)` usa `RenderContext` para culling simples
- `GameTime` e passado explicitamente para `IFase.Draw`

---

## 6) Estado e Navegacao

**StateManager** (`GameMain/Core/StateManager.cs`)
- Mantem `CurrentState`
- Pilha de historico para botao de voltar
- `ResetHistory()` usado em transicoes criticas

**Estados principais**
- Menu
- PreGame
- Load
- ChapterSelect
- Playing
- Victory
- Mapping
- MappingTest
- NewMapMenu
- Settings
- Exit

---

## 7) Servicos (API de Framework)

### 7.1 InputManager
Arquivo: `GameMain/Input/InputManager.cs`

- `Keyboard` -> `Microsoft.Xna.Framework.Input.KeyboardState`
- `Mouse` -> `Microsoft.Xna.Framework.Input.MouseState`
- `GetJoystickState()` -> `SharpDX.DirectInput.JoystickState`

Uso:
- Sempre acessar input via `InputManager`.
- Nunca instanciar DirectInput direto em telas.

### 7.2 TextureCache
Arquivo: `GameMain/Rendering/TextureCache.cs`

- `Pixel` (Texture2D 1x1)
- `GetSolid(Color)`

Regras:
- Nunca criar `Texture2D` dentro de `Draw()`.
- Use `Pixel` + tint para retangulos.

### 7.3 AssetManager
Arquivo: `GameMain/Services/AssetManager.cs`

- `LoadContent<T>(assetName)`
- `LoadTextureFromFile(path)` (com validacao de caminho)
- `CacheTexture(path, texture)`

Regras:
- Texturas externas devem ser copiadas para `Content/Imported`.
- Caminhos fora da pasta do jogo sao bloqueados.

### 7.4 MapService
Arquivo: `GameMain/Services/MapService.cs`

- Cache de `maps.json`
- `RefreshAsync()`
- `GetMapsSnapshot()` / `GetByName()` / `GetMapNamesSnapshot()`
- `SaveMap()` / `DeleteMap()` / `AddMap()`
- `ExportMap()` / `ClearExportedMap()`

### 7.5 SaveService
Arquivo: `GameMain/Services/SaveService.cs`

- Cache de `saves.json`
- `RefreshAsync()`
- `GetSavesSnapshot()`
- `Save()`

---

## 8) Entidades (API de Framework)

### 8.1 Player
Arquivo: `GameMain/Entities/Player.cs`

- Orquestra systems de gameplay e expoe API simples
- Estado principal fica em `PlayerBody` (componente)

Metodos principais:
- `Update(GameTime, IFase)`
- `Draw(GameTime)`
- `ResetPlayer()`
- `SetPosition(Vector2)`
- `SetFacing(bool)`

Systems usados:
- `PlayerInputSystem`
- `PlayerPhysicsSystem`
- `PlayerMovementSystem`
- `PlayerAnimationSystem`
- `PlayerCollisionSystem`

### 8.2 Obstacle
Arquivo: `GameMain/Entities/Obstacle.cs`

- Tipos: `Circle`, `Triangle`, `Square`
- `Draw(SpriteBatch)`
- `CollidesWith(Rectangle)`

### 8.3 Cheese / RatsBurrow
Arquivos: `GameMain/Entities/Cheese.cs`, `GameMain/Entities/RatsBurrow.cs`

- Entidades simples com colisao por retangulo.

---

## 9) Fases (API de Framework)

### 9.1 IFase
Arquivo: `GameMain/Fases/IFase.cs`

Contrato:
- `LoadContent(ContentManager)`
- `Update(Player)`
- `Draw(SpriteBatch, Player, GameTime)`
- `HasWon`, `IsReturning`
- `GetSpawnPosition(bool returning)`

### 9.2 Fase01
Arquivo: `GameMain/Fases/Fase01.cs`

- Exemplo de fase fixa
- Inclui retorno e tela de vitoria

### 9.3 DynamicFase
Arquivo: `GameMain/Fases/DynamicFase.cs`

- Construcao via `MapData`
- Background dinamico
- Obstaculos e coliders do JSON

---

## 10) UI (API de Framework)

Telas principais:
- `MenuScreen`
- `PreGameScreen`
- `LoadScreen`
- `ChapterSelectScreen`
- `NewMapMenuScreen`
- `CreateMappingScreen`
- `MapTestScreen`
- `VictoryScreen`

Regras:
- UI deve apenas renderizar e coletar input.
- Nunca fazer IO direto em Update.
- Nunca dar cast direto para Game1 (usar services).

---

## 11) Editor de Mapas

Arquivo: `GameMain/UI/CreateMappingScreen.cs`

Fluxo de importacao:
1. Usuario escolhe imagem
2. Se arquivo esta fora da pasta do jogo, copia para `Content/Imported`
3. Salva caminho seguro no mapa

Remocao:
- Ao apagar o background, o arquivo em `Content/Imported` e removido
  se nao houver outro layer usando o mesmo arquivo.

---

## 12) Dados (JSON)

### 12.1 maps.json
Arquivo: `Content/maps.json`

Estrutura:
- `MapName`
- `PhaseWidth`
- `ScreenHeight`
- `BackgroundLayers[]`
- `Colliders[]`
- `Obstacles[]`
- `ObstaclesReturn[]`
- `Objects[]`

Enums:
- `ColliderType` = Ground, Platform, Wall, Ceiling
- `ObjectType` = Spawn

### 12.2 saves.json
Arquivo: `Content/saves.json`

Campos:
- `PlayerName`
- `CurrentFaseIndex`
- `IsReturning`

---

## 13) Boas Praticas (Obrigatorio)

- Nunca criar `Texture2D` dentro de `Draw()`
- Sempre usar `deltaTime`
- IO somente via `FileService`/`MapService`/`SaveService`
- Validar caminhos para assets externos
- Evitar LINQ e alocacoes no loop

---

## 14) Extensao (Como adicionar novas features)

### Nova Fase
1. Criar classe em `GameMain/Fases` implementando `IFase`
2. Registrar em `PhaseFactory.CreateFase`

### Nova Tela
1. Criar classe em `GameMain/UI`
2. Instanciar no `GameManager`
3. Registrar transicoes no `StateManager`

### Novo Asset Externo
1. Copiar para `Content/Imported`
2. Registrar no mapa (MapService)

---

## 15) Debug e Diagnostico

- Evite `Console.WriteLine` em Update
- Use logs controlados apenas para debug
- Para testes rapidos: `dotnet watch run`

---

## 16) Roadmap sugerido

- Isolar editor em modulo separado
- Melhorar culling para plataformas e coliders
- Configuracao de resolucao em arquivo unico

---

## 17) Licenca

Defina aqui a licenca do projeto.

