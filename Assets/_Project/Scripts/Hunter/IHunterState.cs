namespace TheOrder.Hunter
{
    /// <summary>
    /// Interface for all Hunter FSM states.
    /// Each state manages its own enter/update/exit logic.
    /// </summary>
    public interface IHunterState
    {
        /// <summary>Called when entering this state.</summary>
        void Enter();

        /// <summary>Called every frame while this state is active.</summary>
        void Update();

        /// <summary>Called when exiting this state.</summary>
        void Exit();
    }
}
