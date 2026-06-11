using System;

namespace BoneThrone.Core
{
    /// <summary>
    /// Placeholder snapshot for the authoritative or local game state.
    /// This phase intentionally avoids collecting real unit, room, HP, cooldown, or level data.
    /// </summary>
    [Serializable]
    public struct GameStateSnapshot
    {
        public GameSessionState SessionState;
        public GameMode Mode;
        public int CurrentLevelIndex;
        public int TurnIndex;
        public RoleId ActiveRole;
        public bool HasSharedKey;

        public static GameStateSnapshot CreateEmpty(GameMode mode)
        {
            return new GameStateSnapshot
            {
                SessionState = GameSessionState.NotStarted,
                Mode = mode,
                CurrentLevelIndex = 0,
                TurnIndex = 0,
                ActiveRole = RoleId.None,
                HasSharedKey = false
            };
        }
    }
}
