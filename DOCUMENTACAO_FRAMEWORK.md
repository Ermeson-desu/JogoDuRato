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
- `GameMain/Rendering`— Cache de texturas e contexto de render/culling
- `GameMain/Utils`    — Utilitarios gerais
- `Content`           — Assets e dados (Content Pipeline + JSON)

Arquivo chave:
- `GameMain/Core/LayoutConfig.cs` — Config central de layout/resolucao (ground, offsets, editor)

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
  - `EditorService`
  - `IGameFlow` (via `GameManager`)
- Instancia e carrega todas as telas
- Controla o fluxo de estados via `StateManager`

**GameManager (IGameFlow)**
- Adaptador de fluxo para a UI (StartNewGame, LoadSave, etc.)

Exemplo (registro de services):
```csharp
// Game1.Initialize
inputManager = new InputManager();
Services.AddService(typeof(InputManager), inputManager);

textureCache = new TextureCache(GraphicsDevice);
Services.AddService(typeof(TextureCache), textureCache);

gameManager = new GameManager(this);
Services.AddService(typeof(IGameFlow), gameManager);
```

---

## 5) Ciclo de Jogo (Loop)

- `Update(GameTime)` sempre usa `deltaTime` (TotalSeconds)
- `Draw(GameTime)` cria `RenderContext` para culling simples
- `RenderContext` e passado explicitamente para `IFase.Draw`
- Culling aplicado para plataformas, obstaculos, queijo e background dinamico

Exemplo (loop de jogo):
```csharp
// Game1.Update
player1.Update(gameTime, levelManager.CurrentFase);
levelManager.Update(player1);
camera.Follow(player1.GetPosition());
```

```csharp
// Game1.Draw
var renderContext = RenderContext.FromCamera(camera, GraphicsDevice.Viewport);
levelManager.Draw(spriteBatch, player1, renderContext);
```

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

Exemplo:
```csharp
var input = game.Services.GetService(typeof(InputManager)) as InputManager;
if (input != null && input.Keyboard.IsKeyDown(Keys.Space))
{
    // pular
}
```

### 7.2 TextureCache
Arquivo: `GameMain/Rendering/TextureCache.cs`

- `Pixel` (Texture2D 1x1)
- `GetSolid(Color)`

Regras:
- Nunca criar `Texture2D` dentro de `Draw()`.
- Use `Pixel` + tint para retangulos.

Exemplo:
```csharp
var cache = game.Services.GetService(typeof(TextureCache)) as TextureCache;
spriteBatch.Draw(cache.Pixel, new Rectangle(10, 10, 100, 10), Color.White);
```

### 7.3 RenderContext
Arquivo: `GameMain/Rendering/RenderContext.cs`

- Calcula bounds visiveis a partir da camera e viewport
- Expande culling para reduzir draw calls fora da tela
- Usa `Camera.ViewOffset` para alinhar o centro da tela
- Padding padrao: `RenderContext.DefaultCullPadding`

Uso:
- Criar no `Draw` (ex: `RenderContext.FromCamera(camera, viewport)`)
- Usar `IsVisible(Rectangle)` antes de desenhar

Exemplo:
```csharp
// Game1.Draw
var renderContext = RenderContext.FromCamera(camera, GraphicsDevice.Viewport);
levelManager.Draw(spriteBatch, player1, renderContext);
```

```csharp
// Em uma fase
if (renderContext.IsVisible(platformRect))
    spriteBatch.Draw(pixel, platformRect, Color.Blue * 0.4f);
```

### 7.4 AssetManager
Arquivo: `GameMain/Services/AssetManager.cs`

- `LoadContent<T>(assetName)`
- `LoadTextureFromFile(path)` (com validacao de caminho)
- `CacheTexture(path, texture)`

Regras:
- Texturas externas devem ser copiadas para `Content/Imported`.
- Caminhos fora da pasta do jogo sao bloqueados.

Exemplo:
```csharp
var assets = game.Services.GetService(typeof(AssetManager)) as AssetManager;
var texture = assets.LoadTextureFromFile(safePath);
```

### 7.5 MapService
Arquivo: `GameMain/Services/MapService.cs`

- Cache de `maps.json`
- `RefreshAsync()`
- `GetMapsSnapshot()` / `GetByName()` / `GetMapNamesSnapshot()`
- `SaveMap()` / `DeleteMap()` / `AddMap()`
- `ExportMap()` / `ClearExportedMap()`

Exemplo:
```csharp
var maps = game.Services.GetService(typeof(MapService)) as MapService;
var data = maps.GetByName("MeuMapa");
```

### 7.6 SaveService
Arquivo: `GameMain/Services/SaveService.cs`

- Cache de `saves.json`
- `RefreshAsync()`
- `GetSavesSnapshot()`
- `Save()`

Exemplo:
```csharp
var saves = game.Services.GetService(typeof(SaveService)) as SaveService;
var list = saves.GetSavesSnapshot();
```

### 7.7 EditorService
Arquivo: `GameMain/Services/EditorService.cs`

- Dialogo de selecao de imagem (assinc)
- Importa imagens para `Content/Imported` (sem travar o loop)
- Remove arquivos importados nao utilizados

Exemplo:
```csharp
// em tela de editor
await editorService.ImportBackgroundAsync(filePath, onImported: path =>
{
    // usar path salvo no mapa
});
```

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

Exemplo:
```csharp
player.Update(gameTime, faseAtual);
player.Draw(gameTime);
```

### 8.2 Obstacle
Arquivo: `GameMain/Entities/Obstacle.cs`

- Tipos: `Circle`, `Triangle`, `Square`
- `Draw(SpriteBatch)`
- `CollidesWith(Rectangle)`

### 8.3 Cheese / RatsBurrow
Arquivos: `GameMain/Entities/Cheese.cs`, `GameMain/Entities/RatsBurrow.cs`

- Entidades simples com colisao por retangulo.

### 8.4 Plataform
Arquivo: `GameMain/Entities/Plataform.cs`

- Entidade simples para plataformas no editor
- Usa `TextureCache.Pixel` para desenhar

Exemplo:
```csharp
var bounds = new Rectangle(1200, 300, 190, LayoutConfig.EditorGroundThickness);
var platform = new Plataform(game, bounds);
platform.Draw(spriteBatch, Color.SteelBlue * 0.6f);
```

---

## 9) Fases (API de Framework)

### 9.1 IFase
Arquivo: `GameMain/Fases/IFase.cs`

Contrato:
- `LoadContent(ContentManager)`
- `Update(Player)`
- `Draw(SpriteBatch, Player, RenderContext)`
- `HasWon`, `IsReturning`
- `GetSpawnPosition(bool returning)`

Exemplo de assinatura:
```csharp
public void Draw(SpriteBatch spriteBatch, Player player, RenderContext renderContext)
{
    // culling simples
    if (renderContext.IsVisible(someBounds))
        spriteBatch.Draw(pixel, someBounds, Color.White);
}
```

### 9.2 Fase01
Arquivo: `GameMain/Fases/Fase01.cs`

- Exemplo de fase fixa
- Inclui retorno e tela de vitoria
- Culling aplicado para plataformas, obstaculos, queijo e buraco

Exemplo de uso de helpers:
```csharp
private static void DrawObstacle(SpriteBatch spriteBatch, RenderContext renderContext, Obstacle obstacle)
{
    if (obstacle != null && renderContext.IsVisible(obstacle.Bounds))
        obstacle.Draw(spriteBatch);
}
```

### 9.3 ReturnStage
Arquivo: `GameMain/Fases/ReturnStage.cs`

- Subfase de retorno (parte final)
- Culling aplicado para plataformas, obstaculos e queijo

### 9.4 DynamicFase
Arquivo: `GameMain/Fases/DynamicFase.cs`

- Construcao via `MapData`
- Background dinamico
- Obstaculos e coliders do JSON
- Culling aplicado para backgrounds, obstaculos e buraco

Exemplo de background com culling:
```csharp
var bounds = new Rectangle((int)layer.StartX, 0, tex.Width, tex.Height);
if (renderContext.IsVisible(bounds))
    spriteBatch.Draw(tex, new Vector2(layer.StartX, 0), Color.White);
```

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

Exemplo (mudanca de estado via IGameFlow):
```csharp
var flow = game.Services.GetService(typeof(IGameFlow)) as IGameFlow;
flow?.StartNewGame(playerName);
```

---

## 11) Editor de Mapas

Arquivo: `GameMain/UI/CreateMappingScreen.cs`

Fluxo de importacao:
1. Usuario escolhe imagem
2. Se arquivo esta fora da pasta do jogo, copia para `Content/Imported`
3. Salva caminho seguro no mapa

Plataformas:
- Disponiveis na paleta lateral como `Plataform`
- Sao salvas como `ColliderType.Platform` em `maps.json`

Exemplo (salvando plataforma):
```csharp
data.Colliders.Add(new ColliderData
{
    Name = "Platform_0",
    Type = ColliderType.Platform,
    Bounds = new RectangleData { X = 1200, Y = 300, Width = 190, Height = 5 }
});
```

Remocao:
- Ao apagar o background, o arquivo em `Content/Imported` e removido
  se nao houver outro layer usando o mesmo arquivo.
  - A remocao usa `EditorService` em background.

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

Enums (tipados no JSON como string):
- `ColliderType` = Ground, Platform, Wall, Ceiling
- `ObjectType` = Spawn

Observacao:
- Valores desconhecidos sao mapeados para `Unknown` no carregamento.

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
- Centralizar tamanhos/offsets em `LayoutConfig` e calcular por `Viewport` quando precisar ser relativo

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
- Configuracao de resolucao em arquivo unico
- Expandir culling para UI pesada e efeitos

---

## 17) Licenca

Defina aqui a licenca do projeto.
