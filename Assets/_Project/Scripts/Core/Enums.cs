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

    public enum KnowledgeLevel
    {
        None,
        Low,
        Medium,
        High
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
        Hard
    }
}
