namespace TheOrder
{
    public enum GameState
    {
        MainMenu,
        Prologue,
        Playing,
        Paused,
        Ending
    }

    public enum HunterState
    {
        Patrol,
        Investigate,
        Chase,
        Search
    }

    public enum ClueCategory
    {
        Truth,
        Mike,
        Weapon
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

    public enum FloorLevel
    {
        Ground,
        Upper,
        Basement
    }
}
