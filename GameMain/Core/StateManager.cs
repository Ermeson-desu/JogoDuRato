namespace GameDuMouse.GameMain.Core
{
    public class StateManager
    {
        public GameState CurrentState { get; private set; } = GameState.Menu;

        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
        }
    }
}