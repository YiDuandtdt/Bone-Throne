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

        [ContextMenu("Phase 7/普通攻击测试")]
        public void BasicAttackTest()
        {
            if (combatSystem == null)
            {
                Debug.LogWarning("CombatInputTester 需要绑定 CombatSystem。", this);
                return;
            }

            bool result = combatSystem.TryBasicAttack(attacker, target);
            Debug.Log("CombatInputTester 普通攻击测试结果：" + result + "。", this);
        }
    }
}
