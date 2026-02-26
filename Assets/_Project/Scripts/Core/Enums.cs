namespace TheOrder
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Death,
        Ending
    }

    public enum HunterState
    {
        Patrol,
        Investigate,
        Chase
    }

    public enum ItemType
    {
        Tool,
        Key
    }

    public enum DifficultyLevel
    {
        Practice,
        Easy,
        Medium,
        Hard,
        Nightmare
    }

    /// <summary>
    /// Tracks the installation state of a car part across deaths.
    /// </summary>
    public enum CarPartState
    {
        None,
        Collected,
        Placed,
        Installed
    }
}
