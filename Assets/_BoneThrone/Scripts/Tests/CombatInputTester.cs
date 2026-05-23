using BoneThrone.Combat;
using BoneThrone.Units;
using UnityEngine;

namespace BoneThrone.Tests
{
    /// <summary>
    /// Temporary Phase 7 ContextMenu helper for GridTest manual combat validation.
    /// This is not formal UI, click input, enemy AI, networking, or production combat flow.
    /// </summary>
    public sealed class CombatInputTester : MonoBehaviour
    {
        [SerializeField] private CombatSystem combatSystem;
        [SerializeField] private Unit attacker;
        [SerializeField] private Unit target;

        [ContextMenu("Phase 7/Basic Attack Test")]
        public void BasicAttackTest()
        {
            if (combatSystem == null)
            {
                Debug.LogWarning("CombatInputTester needs a CombatSystem reference.", this);
                return;
            }

            bool result = combatSystem.TryBasicAttack(attacker, target);
            Debug.Log("CombatInputTester BasicAttackTest result: " + result + ".", this);
        }
    }
}
