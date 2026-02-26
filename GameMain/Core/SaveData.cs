namespace GameDuMouse.GameMain.Core
{
    public class SaveData
    {
        public string PlayerName { get; set; }
        public int CurrentFaseIndex { get; set; }
        public bool IsReturning { get; set; }

        public SaveData() { }

        public SaveData(string playerName, int faseIndex, bool isReturning)
        {
            PlayerName = playerName;
            CurrentFaseIndex = faseIndex;
            IsReturning = isReturning;
        }
    }
}