namespace BoneThrone.Core
{
    /// <summary>
    /// Identifies the session mode without tying gameplay code to a transport layer.
    /// </summary>
    public enum GameMode
    {
        SinglePlayer = 0,
        LANHost = 1,
        LANClient = 2,
        OnlineHost = 3,
        OnlineClient = 4
    }

    /// <summary>
    /// Identifies fixed player roles and non-player turn ownership placeholders.
    /// </summary>
    public enum RoleId
    {
        None = 0,
        Fighter = 1,
        Ranger = 2,
        Mage = 3,
        Barbarian = 4,
        Enemy = 5
    }

    /// <summary>
    /// Describes the intent of a command without implementing any gameplay behavior.
    /// </summary>
    public enum ActionCommandType
    {
        None = 0,
        Move = 1,
        Attack = 2,
        Skill = 3,
        UseItem = 4,
        Defend = 5,
        EndTurn = 6,
        Interact = 7
    }

    /// <summary>
    /// Represents high-level session lifecycle state for local or future network sessions.
    /// </summary>
    public enum GameSessionState
    {
        None = 0,
        NotStarted = 1,
        Starting = 2,
        Running = 3,
        Paused = 4,
        Completed = 5,
        Failed = 6
    }
}
