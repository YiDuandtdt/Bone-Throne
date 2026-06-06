using BoneThrone.Audio;
using BoneThrone.Combat;
using BoneThrone.Turns;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Items
{
    /// <summary>
    /// Executes the local self-use Potion action. It consumes action but never ends the turn.
    /// </summary>
    public sealed class PotionSystem : MonoBehaviour
    {
        private static bool AnimationDebug { get { return false; } }
        private const int DefaultHealAmount = 4;

        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ActionPermissionService actionPermissionService;
        [SerializeField] private CombatLog combatLog;
        [SerializeField] private int healAmount = DefaultHealAmount;

        public bool TryUsePotion(Unit unit)
        {
            ResolveReferences();

            if (unit == null)
            {
                LogRejected("unit is missing.", this);
                return false;
            }

            if (!unit.IsAlive)
            {
                LogRejected("unit is dead.", unit);
                return false;
            }

            if (turnManager == null || actionPermissionService == null)
            {
                LogRejected("turn gate is missing.", unit);
                return false;
            }

            if (turnManager.CurrentPhase != TurnPhase.PlayerTurn)
            {
                LogRejected("current phase is " + turnManager.CurrentPhase + ".", unit);
                return false;
            }

            if (actionPermissionService.TryConsumeStunForAction(unit, turnManager))
            {
                return false;
            }

            if (!actionPermissionService.CanAct(unit, turnManager))
            {
                LogRejected("unit cannot act.", unit);
                return false;
            }

            if (unit.RuntimeState == null || unit.Stats == null)
            {
                LogRejected("runtime HP or stats are missing.", unit);
                return false;
            }

            int maxHp = unit.Stats.GetClampedMaxHp();
            if (unit.RuntimeState.CurrentHp >= maxHp)
            {
                LogRejected("unit is already at full HP.", unit);
                return false;
            }

            UnitPotionState potionState = unit.GetComponent<UnitPotionState>();
            if (potionState == null)
            {
                potionState = unit.gameObject.AddComponent<UnitPotionState>();
            }

            potionState.EnsureInitialized();
            if (!potionState.HasPotion)
            {
                LogRejected("unit has no potions remaining.", unit);
                return false;
            }

            int previousHp = unit.RuntimeState.CurrentHp;
            int nextHp = Mathf.Min(maxHp, previousHp + Mathf.Max(0, healAmount));
            int actualHeal = Mathf.Max(0, nextHp - previousHp);
            if (actualHeal <= 0)
            {
                LogRejected("potion would not restore HP.", unit);
                return false;
            }

            if (!potionState.TryConsumePotion())
            {
                LogRejected("unit has no potions remaining.", unit);
                return false;
            }

            unit.RuntimeState.SetCurrentHp(nextHp);
            MarkActed(unit);
            UnitAnimationController animationController = unit.GetComponent<UnitAnimationController>();
            LogAnimationDebug(unit, animationController, "PlayUsePotion");
            animationController?.PlayUsePotion();
            BTAudioService.PlaySfx(BTAudioCueId.MagicOne);

            if (combatLog != null)
            {
                combatLog.LogPotionUsed(unit, actualHeal, unit.RuntimeState.CurrentHp, maxHp, potionState.CurrentPotionCount);
            }

            turnManager.TryAutoEndPlayerUnitTurnIfNoAvailableActions(unit);
            Debug.Log("PotionSystem: unit " + unit.UnitId + " healed " + actualHeal + " HP.", unit);
            return true;
        }

        private void ResolveReferences()
        {
            if (turnManager == null)
            {
                turnManager = Object.FindFirstObjectByType<TurnManager>();
            }

            if (actionPermissionService == null)
            {
                actionPermissionService = Object.FindFirstObjectByType<ActionPermissionService>();
            }

            if (combatLog == null)
            {
                combatLog = Object.FindFirstObjectByType<CombatLog>();
            }
        }

        private static void MarkActed(Unit unit)
        {
            UnitTurnState turnState = unit != null ? unit.GetComponent<UnitTurnState>() : null;
            if (turnState != null)
            {
                turnState.MarkActed();
            }
        }

        private void LogRejected(string reason, Object context)
        {
            BTAudioService.PlaySfx(BTAudioCueId.InvalidAction);
            if (combatLog != null)
            {
                combatLog.LogPotionRejected(reason, context);
                return;
            }

            Debug.LogWarning("Potion rejected: " + reason, context);
        }

        private void LogAnimationDebug(Unit unit, UnitAnimationController animationController, string method)
        {
            if (!AnimationDebug)
            {
                return;
            }

            Debug.Log(
                "AnimationDebug PotionSystem: unit="
                + (unit != null ? unit.name : "null")
                + " UnitId="
                + (unit != null ? unit.UnitId.ToString() : "n/a")
                + " controller="
                + (animationController != null ? animationController.name : "null")
                + " method="
                + method
                + ".",
                unit);
        }
    }
}
