using System;

namespace BoneThrone.Core
{
    /// <summary>
    /// Minimal local session stub for future singleplayer flow.
    /// It accepts commands and raises placeholder events without executing gameplay rules.
    /// </summary>
    public sealed class LocalGameSession : IGameSession
    {
        private GameStateSnapshot currentSnapshot;

        public LocalGameSession()
        {
            currentSnapshot = GameStateSnapshot.CreateEmpty(GameMode.SinglePlayer);
        }

        public GameMode Mode
        {
            get { return GameMode.SinglePlayer; }
        }

        public GameStateSnapshot CurrentSnapshot
        {
            get { return currentSnapshot; }
        }

        public event Action<ActionCommand> CommandSubmitted;

        public event Action<GameStateSnapshot> SnapshotChanged;

        public void SubmitCommand(ActionCommand command)
        {
            Action<ActionCommand> handler = CommandSubmitted;
            if (handler != null)
            {
                handler(command);
            }
        }

        public void ResetSession()
        {
            currentSnapshot = GameStateSnapshot.CreateEmpty(GameMode.SinglePlayer);
            Action<GameStateSnapshot> handler = SnapshotChanged;
            if (handler != null)
            {
                handler(currentSnapshot);
            }
        }
    }
}
