using System;

namespace BoneThrone.Core
{
    /// <summary>
    /// Data-only command envelope used to describe player or system intent.
    /// It does not validate, execute, move units, roll dice, or send network messages.
    /// </summary>
    [Serializable]
    public struct ActionCommand
    {
        public ActionCommandType CommandType;
        public RoleId ActorRole;
        public int ActorId;
        public int TargetId;
        public int TargetX;
        public int TargetY;
        public int PayloadId;

        public ActionCommand(ActionCommandType commandType, RoleId actorRole, int actorId = 0)
        {
            CommandType = commandType;
            ActorRole = actorRole;
            ActorId = actorId;
            TargetId = 0;
            TargetX = 0;
            TargetY = 0;
            PayloadId = 0;
        }
    }
}
