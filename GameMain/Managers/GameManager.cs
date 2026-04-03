using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.Managers
{
    public sealed class GameManager : IGameFlow
    {
        private readonly Game1 game;

        public GameManager(Game1 game)
        {
            this.game = game;
        }

        public void StartNewGame(string playerName)
        {
            game?.StartNewGame(playerName);
        }

        public void LoadSave(SaveData save)
        {
            game?.LoadSave(save);
        }

        public void PrepareMapEditing(string mapName)
        {
            game?.PrepareMapEditing(mapName);
        }

        public void SetChapterSelection(bool value)
        {
            game?.SetChapterSelection(value);
        }

        public bool TryAdvanceToNextFase()
        {
            return game != null && game.TryAdvanceToNextFase();
        }

        public void SaveProgress(bool isReturning)
        {
            game?.SaveProgress(isReturning);
        }
    }
}
