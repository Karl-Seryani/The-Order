namespace TheOrder
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Ending
    }

    public enum HunterState
    {
        Patrol,
        Investigate,
        Chase
    }

    public enum EndingType
    {
        BlindViolence,
        ConfusedRage,
        HollowEscape,
        GuiltyExecution,
        BitterStandoff,
        BurdenedFlight,
        Fratricide,
        Absolution,
        CowardsExit
    }

    public enum KnowledgeLevel
    {
        None,
        Low,
        Medium,
        High
    }

    public enum EndingChoice
    {
        UseWeapon,
        ConfrontHunter,
        Flee
    }

}
