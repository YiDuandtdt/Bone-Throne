using System;

namespace BoneThrone.Core
{
    /// <summary>
    /// Transport-agnostic session boundary for singleplayer and future multiplayer implementations.
    /// </summary>
    public interface IGameSession
    {
        GameMode Mode { get; }

        GameStateSnapshot CurrentSnapshot { get; }

        event Action<ActionCommand> CommandSubmitted;

        event Action<GameStateSnapshot> SnapshotChanged;

        void SubmitCommand(ActionCommand command);

        void ResetSession();
    }
}
