namespace TheOrder.Hunter
{
    /// <summary>
    /// Plain C# state machine for the Hunter AI.
    /// Manages state transitions, calling Exit/Enter and firing events.
    /// Not a MonoBehaviour — owned and updated by HunterAI.
    /// </summary>
    public class HunterStateMachine
    {
        #region Private Fields

        private IHunterState _currentState;
        private HunterState _currentStateType;

        #endregion

        #region Public API

        /// <summary>The currently active state instance.</summary>
        public IHunterState CurrentState => _currentState;

        /// <summary>The enum type of the current state.</summary>
        public HunterState CurrentStateType => _currentStateType;

        /// <summary>
        /// Transition to a new state. Calls Exit on old, Enter on new,
        /// and fires HunterStateChanged event.
        /// </summary>
        public void ChangeState(IHunterState newState, HunterState stateType)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentStateType = stateType;
            _currentState.Enter();
            GameEvents.HunterStateChanged(stateType);
        }

        /// <summary>Delegates Update to the current state.</summary>
        public void Update()
        {
            _currentState?.Update();
        }

        #endregion
    }
}
