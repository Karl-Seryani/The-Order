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

    public enum ClueCategory
    {
        Truth,
        Mike
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
        ConfrontMike,
        Flee
    }

    public enum SanityEvent
    {
        PassiveDrain,
        SeeingHunter,
        Darkness,
        DisturbingClue,
        ClueRecovery,
        SafeRoom
    }

}
