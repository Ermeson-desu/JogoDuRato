using GameDuMouse.GameMain.Core;

namespace GameDuMouse.GameMain.Managers
{
    public interface IGameFlow
    {
        void StartNewGame(string playerName);
        void LoadSave(SaveData save);
        void PrepareMapEditing(string mapName);
        void SetChapterSelection(bool value);
        bool TryAdvanceToNextFase();
        void SaveProgress(bool isReturning);
    }
}
