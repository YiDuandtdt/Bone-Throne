using BoneThrone.Grid;
using BoneThrone.Units;

namespace BoneThrone.AI
{
    /// <summary>
    /// Small Phase 8 result object for logging one enemy AI action outcome.
    /// It is not a behavior tree, turn state machine, or networking message.
    /// </summary>
    public readonly struct EnemyAIResult
    {
        public readonly bool Success;
        public readonly EnemyAIActionType ActionType;
        public readonly Unit Enemy;
        public readonly Unit Target;
        public readonly GridPosition Destination;
        public readonly string Message;

        private EnemyAIResult(bool success, EnemyAIActionType actionType, Unit enemy, Unit target, GridPosition destination, string message)
        {
            Success = success;
            ActionType = actionType;
            Enemy = enemy;
            Target = target;
            Destination = destination;
            Message = message;
        }

        public static EnemyAIResult Attacked(Unit enemy, Unit target)
        {
            return new EnemyAIResult(true, EnemyAIActionType.Attack, enemy, target, default(GridPosition), "Enemy basic attack resolved.");
        }

        public static EnemyAIResult Moved(Unit enemy, Unit target, GridPosition destination)
        {
            return new EnemyAIResult(true, EnemyAIActionType.Move, enemy, target, destination, "Enemy moved closer to target.");
        }

        public static EnemyAIResult Skipped(Unit enemy, Unit target, string reason)
        {
            return new EnemyAIResult(false, EnemyAIActionType.Skip, enemy, target, default(GridPosition), reason);
        }
    }

    public enum EnemyAIActionType
    {
        Skip = 0,
        Move = 1,
        Attack = 2
    }
}
