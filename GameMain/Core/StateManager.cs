using System.Collections.Generic;

namespace GameDuMouse.GameMain.Core
{
    /// <summary>
    /// Very lightweight state machine used by <see cref="Game1"/>.  In order
    /// to support a generic "back" button (and the Backspace key) we keep a
    /// simple history stack.  Every time the state changes we push the old
    /// value; callers can then call <see cref="GoBack"/> to return to the
    /// previous state.  Additional convenience methods are provided for
    /// clearing the history (e.g. when starting a brand new game).
    /// </summary>
    public class StateManager
    {
        private readonly Stack<GameState> history = new Stack<GameState>();

        public GameState CurrentState { get; private set; } = GameState.Menu;

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
                return;

            history.Push(CurrentState);
            CurrentState = newState;
        }

        /// <summary>
        /// Pop the last state from the history stack and return to it.  If the
        /// stack is empty this method is a no-op.
        /// </summary>
        public void GoBack()
        {
            if (history.Count > 0)
                CurrentState = history.Pop();
        }

        /// <summary>
        /// Clears any stored history.  Useful when starting a new game or when
        /// you want to disable the back button until the next explicit change.
        /// </summary>
        public void ResetHistory()
        {
            history.Clear();
        }
    }
}